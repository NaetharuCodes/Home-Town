using UnityEngine;

// Drop this on the same GameObject as Agent, AgentBrain, and AgentNeeds.
// Fill in the fields in the Inspector, hit Play.
[RequireComponent(typeof(AgentBrain))]
public class AgentBootstrap : MonoBehaviour
{
    [SerializeField] private string agentName = "Alice";
    [SerializeField] private LifeStage lifeStage = LifeStage.YoungAdult;
    [SerializeField] private Vector3Int homeTile;

    [Header("Job (optional)")]
    [SerializeField] private bool hasJob = false;
    [SerializeField] private string jobTitle = "Baker";
    [SerializeField] private float jobSalary = 50f;
    [SerializeField] private Vector3Int workplaceTile;

    private void Start()
    {
        var data = new AgentData(agentName, lifeStage);
        data.HomeTile = homeTile;

        if (hasJob)
            data.Activities.Add(new Job(jobTitle, workplaceTile, jobSalary));

        GetComponent<AgentBrain>().Init(data);
    }
}
