using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "HomeTown/Tile Data")]
public class TileData : ScriptableObject
{
    public bool walkable;
    public float walkCost = 1f;
}