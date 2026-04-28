using System.Collections.Generic;
using UnityEngine;

public class AgentData
{
    public string Name;
    public LifeStage Stage;
    public int AgeDays;
    public float Health; // 0-1, risk of death rises sharply at Elder+

    public Vector3Int HomeTile; // where the agent returns after scheduled activities

    public Dictionary<string, float> Traits = new();
    public List<AgentActivity> Activities = new();

    public AgentData(string name, LifeStage stage = LifeStage.YoungAdult)
    {
        Name = name;
        Stage = stage;
        AgeDays = 0;
        Health = 1f;
    }

    public bool HasTrait(string trait) => Traits.ContainsKey(trait);

    public float GetTrait(string trait, float defaultValue = 0f)
        => Traits.TryGetValue(trait, out float v) ? v : defaultValue;
}
