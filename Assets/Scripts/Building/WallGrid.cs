using System;
using System.Collections.Generic;
using UnityEngine;

public class WallGrid
{
    // Dictionary per floor: floor level → cell position → Wall
    private readonly Dictionary<int, Dictionary<Vector3Int, Wall>> wallsByFloor = new();

    public WallGrid()
    {
        wallsByFloor[-1] = new();
        wallsByFloor[0] = new();
        wallsByFloor[1] = new();
    }

    public void AddWall(Wall wall)
    {
        if (!wallsByFloor.ContainsKey(wall.floorLevel))
            wallsByFloor[wall.floorLevel] = new();

        wallsByFloor[wall.floorLevel][wall.position] = wall;
    }

    public void RemoveWall(Vector3Int position, int floor)
    {
        if (wallsByFloor.TryGetValue(floor, out var dict))
            dict.Remove(position);
    }

    public Wall GetWall(Vector3Int position, int floor)
    {
        if (wallsByFloor.TryGetValue(floor, out var dict) && dict.TryGetValue(position, out var wall))
            return wall;
        return null;
    }

    public bool HasWall(Vector3Int position, int floor)
        => GetWall(position, floor) != null;

    public bool IsWallBlocking(Vector3Int from, Vector3Int to, int floor, DateTime currentTime)
    {
        Wall wall = GetWall(to, floor);
        if (wall == null) return false;

        // Wall blocks if it's UnderConstruction or Completed
        return wall.state == WallState.UnderConstruction || wall.state == WallState.Completed;
    }

    public List<Wall> GetWallsInState(WallState state, int floor)
    {
        var result = new List<Wall>();
        if (!wallsByFloor.TryGetValue(floor, out var dict)) return result;

        foreach (var wall in dict.Values)
        {
            if (wall.state == state)
                result.Add(wall);
        }
        return result;
    }

    public List<Wall> GetAllWalls(int floor)
    {
        var result = new List<Wall>();
        if (wallsByFloor.TryGetValue(floor, out var dict))
            result.AddRange(dict.Values);
        return result;
    }

    public void Clear()
    {
        foreach (var dict in wallsByFloor.Values)
            dict.Clear();
    }

    public int GetWallCount(int floor)
    {
        if (wallsByFloor.TryGetValue(floor, out var dict))
            return dict.Count;
        return 0;
    }
}
