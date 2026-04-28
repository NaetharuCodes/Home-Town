using System.Collections.Generic;
using UnityEngine;

public class WorldObjectRegistry : MonoBehaviour
{
    public static WorldObjectRegistry Instance { get; private set; }

    private readonly List<InteractableObject> objects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(InteractableObject obj) => objects.Add(obj);
    public void Unregister(InteractableObject obj) => objects.Remove(obj);

    public (InteractableObject obj, ObjectProperty prop) FindBest(
        string tag, string preferOwner, Vector3 agentPosition)
    {
        InteractableObject bestObj = null;
        ObjectProperty bestProp = null;
        float bestScore = float.MinValue;

        foreach (var obj in objects)
        {
            if (obj.IsInUse) continue;

            var prop = obj.GetProperty(tag);
            if (prop == null) continue;

            float distance = Vector3.Distance(agentPosition, obj.transform.position);
            bool isOwned = !string.IsNullOrEmpty(preferOwner) && obj.OwnerName == preferOwner;

            // Owned objects always win over unowned. Among equals, quality then proximity.
            float score = (isOwned ? 1000f : 0f) + prop.Quality * 10f - distance;

            if (score > bestScore)
            {
                bestScore = score;
                bestObj = obj;
                bestProp = prop;
            }
        }

        return (bestObj, bestProp);
    }
}
