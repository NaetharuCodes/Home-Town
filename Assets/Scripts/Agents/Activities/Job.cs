using System;
using System.Collections.Generic;
using UnityEngine;

public class Job : AgentActivity
{
    public string Title;
    public Vector3Int WorkplaceTile;
    public float Salary;
    public float Performance = 0.5f; // 0-1, can affect promotions/firing later

    public int StartHour = 9;
    public int EndHour = 17;

    private static readonly DayOfWeek[] WorkDays =
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday
    };

    public Job(string title, Vector3Int workplaceTile, float salary)
    {
        Title = title;
        WorkplaceTile = workplaceTile;
        Salary = salary;
    }

    public override IEnumerable<ScheduledAction> GetSchedule(AgentData agent)
    {
        float hoursAtWork = EndHour - StartHour;
        float duration = (hoursAtWork / 24f) * GameTimeManager.Instance.SecondsPerGameDay;

        yield return new ScheduledAction
        {
            Description = $"Go to work ({Title})",
            ActiveDays = WorkDays,
            Destination = WorkplaceTile,
            NeedFulfilled = NeedType.Achievement,
            FulfillmentAmount = 0.5f,
            IsRabbitHole = true,
            RabbitHoleDuration = duration
        };
    }

    public override void OnDayTick(AgentData agent, AgentNeeds needs)
    {
        // Trait example: ambitious agents get a daily achievement bonus
        if (agent.HasTrait("ambitious"))
            needs.Fulfill(NeedType.Achievement, 0.1f);
    }
}
