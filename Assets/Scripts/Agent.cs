using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private int currentFloor = 0;
    [SerializeField] private float moveSpeed = 3f;

    public int CurrentFloor => currentFloor;

    private SpriteRenderer spriteRenderer;
    private Coroutine moveRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        FloorManager.Instance.OnViewFloorChanged += UpdateVisibility;
    }

    private void OnDisable()
    {
        if (FloorManager.Instance != null)
            FloorManager.Instance.OnViewFloorChanged -= UpdateVisibility;
    }

    public void MoveTo(Vector3Int goalCell)
    {
        Vector3Int startCell = WorldToCell(transform.position);
        List<Vector3Int> path = AStarPathfinder.FindPath(startCell, goalCell, currentFloor);
        if (path == null || path.Count <= 1) return;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(FollowPath(path));
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        worldPos.z = 0f;
        return FloorManager.Instance.GetTilemap(currentFloor).WorldToCell(worldPos);
    }

    private IEnumerator FollowPath(List<Vector3Int> path)
    {
        var tilemap = FloorManager.Instance.GetTilemap(currentFloor);

        // path[0] is the cell the agent is already on — skip it
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 target = tilemap.GetCellCenterWorld(path[i]);
            target.z = transform.position.z;

            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
        }

        moveRoutine = null;
    }

    private void UpdateVisibility(int viewFloor)
    {
        if (spriteRenderer == null) return;
        bool visible = currentFloor <= viewFloor;
        Color c = spriteRenderer.color;
        c.a = visible ? 1f : 0f;
        spriteRenderer.color = c;
    }
}
