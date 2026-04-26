using UnityEngine;
using UnityEngine.InputSystem;

// Temporary test script — attach to the Agent GameObject.
// Click anywhere in the scene to send the agent to that tile.
public class ClickToMove : MonoBehaviour
{
    private Agent agent;
    private Camera mainCam;

    private void Awake()
    {
        agent = GetComponent<Agent>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        worldPos.z = 0f;
        Vector3Int cell = agent.WorldToCell(worldPos);
        agent.MoveTo(cell);
    }
}
