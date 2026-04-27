using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBlueprint
{
    public List<Wall> plannedWalls = new();
    public DateTime scheduledStartDate;
    public DateTime estimatedCompletionDate;
    public int daysToComplete;
    public float totalCost;
    public bool isLocked;

    public BuildingBlueprint()
    {
        isLocked = false;
        totalCost = 0f;
    }

    public void AddWall(Wall wall)
    {
        plannedWalls.Add(wall);
    }

    public void RemoveWall(Wall wall)
    {
        plannedWalls.Remove(wall);
    }

    public void CalculateDuration(int daysPerTile = 1)
    {
        daysToComplete = plannedWalls.Count * daysPerTile;
    }

    public void LockBlueprint(DateTime startDate, int daysPerTile = 1)
    {
        if (isLocked) return;
        isLocked = true;
        CalculateDuration(daysPerTile);
        scheduledStartDate = startDate;
        estimatedCompletionDate = startDate.AddDays(daysToComplete);
    }
}
