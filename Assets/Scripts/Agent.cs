using System;
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
    private bool rabbitHoleHidden = false;
    private int lastViewFloor = 0;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        lastViewFloor = FloorManager.Instance.ViewFloor;
        FloorManager.Instance.OnViewFloorChanged += OnViewFloorChanged;
    }

    private void OnDisable()
    {
        if (FloorManager.Instance != null)
            FloorManager.Instance.OnViewFloorChanged -= OnViewFloorChanged;
    }

    public void MoveTo(Vector3Int goalCell, Action onArrival = null)
    {
        Vector3Int startCell = WorldToCell(transform.position);
        List<Vector3Int> path = AStarPathfinder.FindPath(startCell, goalCell, currentFloor);
        if (path == null || path.Count <= 1)
        {
            onArrival?.Invoke();
            return;
        }

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(FollowPath(path, onArrival));
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        worldPos.z = 0f;
        var tilemap = FloorManager.Instance?.GetTilemap(currentFloor);
        if (tilemap == null)
        {
            Debug.LogError($"[Agent] No tilemap for floor {currentFloor} — check FloorManager inspector assignments.");
            return Vector3Int.zero;
        }
        return tilemap.WorldToCell(worldPos);
    }

    // Hides the agent sprite for rabbit hole activities
    public void SetRabbitHoleHidden(bool hidden)
    {
        rabbitHoleHidden = hidden;
        ApplyVisibility();
    }

    private void OnViewFloorChanged(int viewFloor)
    {
        lastViewFloor = viewFloor;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (spriteRenderer == null) return;

        if (rabbitHoleHidden)
        {
            spriteRenderer.enabled = false;
            return;
        }

        spriteRenderer.enabled = true;
        Color c = spriteRenderer.color;
        c.a = currentFloor <= lastViewFloor ? 1f : 0f;
        spriteRenderer.color = c;
    }

    private IEnumerator FollowPath(List<Vector3Int> path, Action onArrival)
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
        onArrival?.Invoke();
    }
}
