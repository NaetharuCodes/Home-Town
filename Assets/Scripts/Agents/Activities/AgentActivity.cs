using System.Collections.Generic;

public abstract class AgentActivity
{
    // Scheduled actions the agent must do on specific days (work, school, etc.)
    public abstract IEnumerable<ScheduledAction> GetSchedule(AgentData agent);

    // Called every game-day tick — override for ongoing effects (salary, skill gain, etc.)
    public virtual void OnDayTick(AgentData agent, AgentNeeds needs) { }
}
