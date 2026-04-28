using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private List<ObjectProperty> properties = new();

    [SerializeField] private int floor = 0;

    // Offset in tiles from the object's position where the agent stands to use it.
    // (0,0) = on the object (beds, chairs, toilets).
    // (0,-1) = one tile south (fridges, sinks against a north wall), etc.
    [SerializeField] private Vector2Int useTileOffset = Vector2Int.zero;

    public Vector3Int UseTile
    {
        get
        {
            var tilemap = FloorManager.Instance?.GetTilemap(floor);
            if (tilemap == null) return Vector3Int.zero;
            var objectTile = tilemap.WorldToCell(transform.position);
            return new Vector3Int(objectTile.x + useTileOffset.x, objectTile.y + useTileOffset.y, 0);
        }
    }

    // Null/empty means any agent can use it; set to an agent name to make it owned
    [SerializeField] private string ownerName;
    public string OwnerName => ownerName;

    public bool IsInUse { get; private set; }

    private bool started = false;

    // Use Start (not OnEnable) for first registration so WorldObjectRegistry.Awake
    // is guaranteed to have run first. OnEnable re-registers if the object is toggled.
    private void Start()
    {
        started = true;
        Register();
    }

    private void OnEnable()
    {
        if (started) Register();
    }

    private void OnDisable()
    {
        if (WorldObjectRegistry.Instance != null)
            WorldObjectRegistry.Instance.Unregister(this);
    }

    private void Register()
    {
        if (WorldObjectRegistry.Instance == null)
        {
            Debug.LogError($"[InteractableObject] No WorldObjectRegistry in scene — {name} will not be usable.");
            return;
        }
        WorldObjectRegistry.Instance.Register(this);
    }

    public bool HasTag(string tag)
    {
        foreach (var p in properties)
            if (p.Tag == tag) return true;
        return false;
    }

    // Returns the property for the given tag, or null if this object doesn't support it
    public ObjectProperty GetProperty(string tag)
    {
        foreach (var p in properties)
            if (p.Tag == tag) return p;
        return null;
    }

    public void Claim() => IsInUse = true;
    public void Release() => IsInUse = false;
}
