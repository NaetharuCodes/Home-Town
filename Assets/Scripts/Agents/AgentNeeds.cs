using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentNeeds : MonoBehaviour
{
    [Serializable]
    private class NeedsBlock
    {
        [Range(0,1)] public float Hunger       = 1f;
        [Range(0,1)] public float Thirst       = 1f;
        [Range(0,1)] public float Toilet       = 1f;
        [Range(0,1)] public float Sleep        = 1f;
        [Range(0,1)] public float Fun          = 1f;
        [Range(0,1)] public float Achievement  = 1f;
        [Range(0,1)] public float Companionship = 1f;

        public float Get(NeedType need) => need switch
        {
            NeedType.Hunger        => Hunger,
            NeedType.Thirst        => Thirst,
            NeedType.Toilet        => Toilet,
            NeedType.Sleep         => Sleep,
            NeedType.Fun           => Fun,
            NeedType.Achievement   => Achievement,
            NeedType.Companionship => Companionship,
            _                      => 0f
        };

        public void Set(NeedType need, float value)
        {
            switch (need)
            {
                case NeedType.Hunger:        Hunger        = value; break;
                case NeedType.Thirst:        Thirst        = value; break;
                case NeedType.Toilet:        Toilet        = value; break;
                case NeedType.Sleep:         Sleep         = value; break;
                case NeedType.Fun:           Fun           = value; break;
                case NeedType.Achievement:   Achievement   = value; break;
                case NeedType.Companionship: Companionship = value; break;
            }
        }

        public NeedType MostUrgent()
        {
            NeedType worst = NeedType.Hunger;
            float min = Hunger;

            void Check(NeedType n, float v) { if (v < min) { min = v; worst = n; } }
            Check(NeedType.Thirst,        Thirst);
            Check(NeedType.Toilet,        Toilet);
            Check(NeedType.Sleep,         Sleep);
            Check(NeedType.Fun,           Fun);
            Check(NeedType.Achievement,   Achievement);
            Check(NeedType.Companionship, Companionship);

            return worst;
        }
    }

    [SerializeField] private NeedsBlock current = new();

    private readonly Dictionary<NeedType, float> decayRates = new()
    {
        { NeedType.Hunger,        0.15f },
        { NeedType.Thirst,        0.20f },
        { NeedType.Toilet,        0.18f },
        { NeedType.Sleep,         0.12f },
        { NeedType.Fun,           0.08f },
        { NeedType.Achievement,   0.05f },
        { NeedType.Companionship, 0.06f },
    };

    private readonly Dictionary<NeedType, float> thresholds = new()
    {
        { NeedType.Hunger,        0.40f },
        { NeedType.Thirst,        0.35f },
        { NeedType.Toilet,        0.25f },
        { NeedType.Sleep,         0.30f },
        { NeedType.Fun,           0.20f },
        { NeedType.Achievement,   0.15f },
        { NeedType.Companionship, 0.15f },
    };

    private void Start()
    {
        GameTimeManager.Instance.OnTimeChanged += OnDayTick;
    }

    private void OnDisable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= OnDayTick;
    }

    private void OnDayTick(DateTime _)
    {
        foreach (NeedType n in Enum.GetValues(typeof(NeedType)))
            current.Set(n, Mathf.Clamp01(current.Get(n) - decayRates[n]));
    }

    public float Get(NeedType need) => current.Get(need);

    public void Fulfill(NeedType need, float amount)
        => current.Set(need, Mathf.Clamp01(current.Get(need) + amount));

    public bool IsUrgent(NeedType need) => current.Get(need) < thresholds[need];

    public NeedType MostUrgent() => current.MostUrgent();
}
