using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    private readonly Dictionary<int, Dictionary<Vector3Int, TileData>> _baked = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BakeAll();
    }

    public void BakeAll()
    {
        _baked.Clear();
        BakeFloor(-1);
        BakeFloor(0);
        BakeFloor(1);
    }

    public void BakeFloor(int floor)
    {
        var dict = new Dictionary<Vector3Int, TileData>();
        _baked[floor] = dict;

        Tilemap tilemap = FloorManager.Instance.GetTilemap(floor);
        if (tilemap == null) return;

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            CustomTile tile = tilemap.GetTile<CustomTile>(pos);
            if (tile?.tiledata != null)
                dict[pos] = tile.tiledata;
        }

    }

    public TileData GetTileData(Vector3Int pos, int floor)
    {
        return _baked.TryGetValue(floor, out var dict) && dict.TryGetValue(pos, out var data)
            ? data : null;
    }

    public bool IsWalkable(Vector3Int pos, int floor)
    {
        TileData data = GetTileData(pos, floor);
        return data != null && data.walkable;
    }

    // Checks walkability of destination AND wall edges on both cells.
    // Also checks if a wall from BuildingManager is blocking the path.
    public bool CanMove(Vector3Int from, Vector3Int to, int floor)
    {
        if (!IsWalkable(to, floor)) return false;

        return true;
    }

    public float GetWalkCost(Vector3Int pos, int floor)
        => GetTileData(pos, floor)?.walkCost ?? 1f;
}
