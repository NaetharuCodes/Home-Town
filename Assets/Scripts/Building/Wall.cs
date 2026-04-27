using UnityEngine;

public enum WallState { Planned, UnderConstruction, Completed }
public enum WallMaterial { Wood, Brick, Stone, Concrete }
public enum TileType { Dirt, Grass, Stone, Concrete, Wood }

public class Wall
{
    public Vector3Int position;
    public WallMaterial material;
    public WallState state;
    public System.DateTime completionDate; // When construction finishes
    public int floorLevel; // -1, 0, or 1
    public TileType builtOn; // What tile is underneath
    public TileType replacedTile; // What to show after build (e.g., Concrete if built on Dirt)

    public Wall(Vector3Int position, int floorLevel, WallMaterial material, TileType builtOn, TileType replacedTile)
    {
        this.position = position;
        this.floorLevel = floorLevel;
        this.material = material;
        this.state = WallState.Planned;
        this.builtOn = builtOn;
        this.replacedTile = replacedTile;
    }
}
