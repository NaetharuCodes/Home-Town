using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private int currentFloor = 0;
    public int CurrentFloor => currentFloor;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); ;
    }

    private void Start()
    {
        FloorManager.Instance.OnViewFloorChanged += UpdateVisibility;
    }

    private void OnDisable()
    {
        if (FloorManager.Instance != null)
            FloorManager.Instance.OnViewFloorChanged -= UpdateVisibility;
    }

    private void UpdateVisibility(int viewFloor)
    {
        if (spriteRenderer == null) return;

        bool visible = currentFloor <= viewFloor;
        Color c = spriteRenderer.color;
        c.a = visible ? 1f : 0f;
        spriteRenderer.color = c;
    }

}