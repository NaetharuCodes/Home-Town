using System;
using UnityEngine;

public class ScheduledAction
{
    public string Description;
    public DayOfWeek[] ActiveDays;
    public int StartHour;   // 0-23
    public int StartMinute; // 0-59
    public int EndHour;     // 0-23
    public Vector3Int Destination;
    public NeedType NeedFulfilled;
    public float FulfillmentAmount;
    public bool IsRabbitHole;
    public float RabbitHoleDuration; // real seconds, computed from hours * secondsPerHour

    public bool IsActiveOn(DayOfWeek day)
    {
        foreach (var d in ActiveDays)
            if (d == day) return true;
        return false;
    }

    // True if the current time is at or past the start time for today
    public bool IsTimeToStart(DateTime now)
    {
        int nowMinutes   = now.Hour * 60 + now.Minute;
        int startMinutes = StartHour * 60 + StartMinute;
        int endMinutes   = EndHour * 60;
        return nowMinutes >= startMinutes && nowMinutes < endMinutes;
    }
}
