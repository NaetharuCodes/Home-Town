using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "HomeTown/Tile Data")]
public class TileData : ScriptableObject
{
    public bool walkable;
    public float walkCost = 1f;

    // WALLS
    public bool wallNorth;
    public bool wallEast;
    public bool wallSouth;
    public bool wallWest;

    public bool HasWall(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return wallNorth;
        if (direction == Vector2Int.right) return wallEast;
        if (direction == Vector2Int.down) return wallSouth;
        if (direction == Vector2Int.left) return wallWest;

        return false;
    }
}