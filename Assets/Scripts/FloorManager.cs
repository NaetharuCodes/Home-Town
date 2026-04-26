using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    [SerializeField] private Tilemap basementTilemap;
    [SerializeField] private Tilemap groundFloorTilemap;
    [SerializeField] private Tilemap firstFloorTilemap;

    [SerializeField] private TilemapRenderer basementRenderer;
    [SerializeField] private TilemapRenderer groundFloorRenderer;
    [SerializeField] private TilemapRenderer firstFloorRenderer;

    public event System.Action<int> OnViewFloorChanged;

    private int viewFloor = 0;
    public int ViewFloor => viewFloor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetViewFloor(-1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetViewFloor(0);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetViewFloor(1);
    }

    public void SetViewFloor(int floor)
    {
        viewFloor = floor;
        UpdateTilemapVisibility();
        OnViewFloorChanged?.Invoke(floor);
    }

    private void UpdateTilemapVisibility()
    {
        SetFloorVisibility(basementTilemap, basementRenderer, -1);
        SetFloorVisibility(groundFloorTilemap, groundFloorRenderer, 0);
        SetFloorVisibility(firstFloorTilemap, firstFloorRenderer, 1);
    }

    public Tilemap GetTilemap(int floor) => floor switch
    {
        -1 => basementTilemap,
         0 => groundFloorTilemap,
         1 => firstFloorTilemap,
         _ => null
    };

    private void SetFloorVisibility(Tilemap tilemap, TilemapRenderer renderer, int floorLevel)
    {
        // Hide all the floors that are above my current set floor
        bool visible = floorLevel <= viewFloor;
        Color c = tilemap.color;
        c.a = visible ? 1f : 0f;
        tilemap.color = c;

        // Sort my higher floors so that they are up on the top
        renderer.sortingOrder = floorLevel;
    }
}
