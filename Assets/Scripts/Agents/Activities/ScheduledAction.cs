using System;
using UnityEngine;

public class ScheduledAction
{
    public string Description;
    public DayOfWeek[] ActiveDays;
    public Vector3Int Destination;
    public NeedType NeedFulfilled;
    public float FulfillmentAmount;

    // When true the agent vanishes at Destination for RabbitHoleDuration real seconds, then walks home
    public bool IsRabbitHole;
    public float RabbitHoleDuration; // real seconds, computed from work hours + game speed

    public bool IsActiveOn(DayOfWeek day)
    {
        foreach (var d in ActiveDays)
            if (d == day) return true;
        return false;
    }
}
