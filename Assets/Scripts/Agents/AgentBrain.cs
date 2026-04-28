using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
[RequireComponent(typeof(AgentNeeds))]
public class AgentBrain : MonoBehaviour
{
    private enum State { Free, Busy }

    private Agent agent;
    private AgentNeeds needs;
    private State state = State.Free;

    // Tracks which scheduled actions have already run today
    private readonly HashSet<string> completedToday = new();
    private DayOfWeek lastKnownDay;

    // Critical thresholds — override obligations (agent won't go to work if passing out)
    private static readonly Dictionary<NeedType, float> CriticalThresholds = new()
    {
        { NeedType.Hunger,  0.15f },
        { NeedType.Thirst,  0.10f },
        { NeedType.Toilet,  0.10f },
        { NeedType.Sleep,   0.10f },
    };

    private static readonly Dictionary<NeedType, string> NeedTags = new()
    {
        { NeedType.Hunger,        ObjectTags.Food          },
        { NeedType.Thirst,        ObjectTags.Drink         },
        { NeedType.Sleep,         ObjectTags.Sleep         },
        { NeedType.Toilet,        ObjectTags.Toilet        },
        { NeedType.Fun,           ObjectTags.Entertainment },
        { NeedType.Companionship, ObjectTags.Social        },
    };

    public AgentData Data { get; private set; }

    private void Awake()
    {
        agent = GetComponent<Agent>();
        needs = GetComponent<AgentNeeds>();
    }

    public void Init(AgentData data)
    {
        Data = data;
        lastKnownDay = GameTimeManager.Instance.CurrentTime.DayOfWeek;
    }

    private void Start()
    {
        GameTimeManager.Instance.OnDayChanged += OnDayTick;
        Decide();
    }

    private void OnDisable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged -= OnDayTick;
    }

    private void OnDayTick(DateTime currentTime)
    {
        if (Data == null) return;

        if (currentTime.DayOfWeek != lastKnownDay)
        {
            completedToday.Clear();
            lastKnownDay = currentTime.DayOfWeek;
        }

        foreach (var activity in Data.Activities)
            activity.OnDayTick(Data, needs);

        // Re-evaluate in case obligations just became available
        if (state == State.Free)
            Decide();
    }

    // Central decision point — called at startup and at the end of every action.
    private void Decide()
    {
        if (state == State.Busy || Data == null) return;

        if (TryCriticalNeed())       return;
        if (TryObligation())         return;
        if (TryRegularNeed())        return;
        if (TryLowPriorityNeed())    return;

        // Final fallback — agents should never just stand still
        StartCoroutine(WanderRoutine());
    }

    // --- Decision checks ---

    private bool TryCriticalNeed()
    {
        NeedType worst = NeedType.Hunger;
        float min = float.MaxValue;
        bool found = false;

        foreach (var kvp in CriticalThresholds)
        {
            float val = needs.Get(kvp.Key);
            if (val < kvp.Value && val < min)
            {
                min = val;
                worst = kvp.Key;
                found = true;
            }
        }

        return found && TryUseObjectForNeed(worst);
    }

    private bool TryObligation()
    {
        DateTime now = GameTimeManager.Instance.CurrentTime;

        foreach (var activity in Data.Activities)
        {
            foreach (var scheduled in activity.GetSchedule(Data))
            {
                if (!scheduled.IsActiveOn(now.DayOfWeek)) continue;
                if (completedToday.Contains(scheduled.Description)) continue;
                if (!scheduled.IsTimeToStart(now)) continue;

                ExecuteScheduledAction(scheduled);
                return true;
            }
        }

        return false;
    }

    private bool TryRegularNeed()
    {
        NeedType urgent = needs.MostUrgent();
        if (!needs.IsUrgent(urgent)) return false;
        return TryUseObjectForNeed(urgent);
    }

    private bool TryUseObjectForNeed(NeedType need)
    {
        if (!NeedTags.TryGetValue(need, out string tag)) return false;

        if (WorldObjectRegistry.Instance == null)
        {
            Debug.LogError($"[{Data.Name}] WorldObjectRegistry is missing from the scene!");
            return false;
        }

        var (obj, prop) = WorldObjectRegistry.Instance.FindBest(tag, Data.Name, transform.position);
        if (obj != null)
        {
            StartCoroutine(UseObjectRoutine(obj, prop));
            return true;
        }

        if (need == NeedType.Sleep)
        {
            StartCoroutine(SleepOnFloorRoutine());
            return true;
        }

        Debug.Log($"[{Data.Name}] Needs {need} but no object available");
        return false;
    }

    // --- Action coroutines ---

    private void ExecuteScheduledAction(ScheduledAction action)
    {
        state = State.Busy;
        Debug.Log($"[{Data.Name}] {action.Description}");

        if (action.IsRabbitHole)
            agent.MoveTo(action.Destination, () => StartCoroutine(RabbitHoleRoutine(action)));
        else
        {
            needs.Fulfill(action.NeedFulfilled, action.FulfillmentAmount);
            completedToday.Add(action.Description);
            state = State.Free;
            Decide();
        }
    }

    private IEnumerator RabbitHoleRoutine(ScheduledAction action)
    {
        agent.SetRabbitHoleHidden(true);
        needs.Fulfill(action.NeedFulfilled, action.FulfillmentAmount);

        yield return new WaitForSeconds(action.RabbitHoleDuration);

        agent.SetRabbitHoleHidden(false);
        completedToday.Add(action.Description);

        bool arrived = false;
        agent.MoveTo(Data.HomeTile, () => arrived = true);
        yield return new WaitUntil(() => arrived);

        state = State.Free;
        Decide();
    }

    private IEnumerator UseObjectRoutine(InteractableObject obj, ObjectProperty prop)
    {
        state = State.Busy;
        obj.Claim();

        bool arrived = false;
        agent.MoveTo(obj.UseTile, () => arrived = true);
        yield return new WaitUntil(() => arrived);

        Debug.Log($"[{Data.Name}] Using {obj.name} ({prop.Tag})");
        yield return new WaitForSeconds(prop.UseDuration);

        needs.Fulfill(prop.NeedFulfilled, prop.FulfillmentAmount);
        obj.Release();

        state = State.Free;
        Decide();
    }

    private IEnumerator SleepOnFloorRoutine()
    {
        state = State.Busy;
        float duration = GameTimeManager.Instance.SecondsPerGameDay * 0.33f;
        Debug.Log($"[{Data.Name}] Sleeping on the floor");
        yield return new WaitForSeconds(duration);

        needs.Fulfill(NeedType.Sleep, 0.4f);
        state = State.Free;
        Decide();
    }

    private bool TryLowPriorityNeed()
    {
        // Check Fun and Companionship even if not technically "urgent" yet —
        // agents seek these proactively when nothing more pressing is happening
        foreach (var need in new[] { NeedType.Fun, NeedType.Companionship, NeedType.Achievement })
        {
            if (NeedTags.TryGetValue(need, out string tag) && WorldObjectRegistry.Instance != null)
            {
                var (obj, prop) = WorldObjectRegistry.Instance.FindBest(tag, Data.Name, transform.position);
                if (obj != null && needs.Get(need) < 0.8f)
                {
                    StartCoroutine(UseObjectRoutine(obj, prop));
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator WanderRoutine()
    {
        state = State.Busy;

        // Pick a random tile within a small radius and walk there
        Vector3Int current = agent.WorldToCell(transform.position);
        Vector3Int target = new Vector3Int(
            current.x + UnityEngine.Random.Range(-4, 5),
            current.y + UnityEngine.Random.Range(-4, 5),
            0
        );

        bool arrived = false;
        agent.MoveTo(target, () => arrived = true);

        // Wait for arrival or timeout (target tile may be unwalkable)
        float timeout = 5f;
        float elapsed = 0f;
        while (!arrived && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        state = State.Free;
        Decide();
    }
}
