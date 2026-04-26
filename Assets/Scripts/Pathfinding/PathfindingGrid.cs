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
    // You can't walk if either the current cell has an exit wall or the next cell has an entry wall
    // that sits between you and it!
    public bool CanMove(Vector3Int from, Vector3Int to, int floor)
    {
        if (!IsWalkable(to, floor)) return false;

        var dir = new Vector2Int(to.x - from.x, to.y - from.y);
        TileData fromData = GetTileData(from, floor);
        TileData toData = GetTileData(to, floor);

        if (fromData != null && fromData.HasWall(dir)) return false;
        if (toData != null && toData.HasWall(new Vector2Int(-dir.x, -dir.y))) return false;

        return true;
    }

    public float GetWalkCost(Vector3Int pos, int floor)
        => GetTileData(pos, floor)?.walkCost ?? 1f;
}
