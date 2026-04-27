using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private WallGrid wallGrid = new();
    private BuildingBlueprint activeBlueprint;
    private bool blueprintModeActive = false;

    [SerializeField] private CustomTile wallTilePrefab;

    public event System.Action OnBlueprintLocked;
    public event System.Action OnConstructionStarted;
    public event System.Action OnConstructionCompleted;
    public event System.Action OnArchitectArrived;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GameTimeManager.Instance.OnTimeChanged += CheckConstructionDates;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= CheckConstructionDates;
    }

    /// <summary>
    /// Architect has arrived. Enable blueprint mode for player to design.
    /// </summary>
    public void StartBlueprintMode()
    {
        blueprintModeActive = true;
        activeBlueprint = new BuildingBlueprint();

        // Disable click-to-move during blueprint design
        ClickToMove clickToMove = FindObjectOfType<ClickToMove>();
        if (clickToMove) clickToMove.enabled = false;

        OnArchitectArrived?.Invoke();
        Debug.Log("[BuildingManager] Blueprint mode started. Player can now design.");
    }

    /// <summary>
    /// Add a wall to the current blueprint.
    /// </summary>
    public void AddWallToBlueprint(Vector3Int position, int floor, WallMaterial material, TileType builtOn, TileType replacedTile)
    {
        if (!blueprintModeActive || activeBlueprint == null) return;

        var wall = new Wall(position, floor, material, builtOn, replacedTile);
        activeBlueprint.AddWall(wall);
    }

    /// <summary>
    /// Remove a wall from the current blueprint.
    /// </summary>
    public void RemoveWallFromBlueprint(Vector3Int position, int floor)
    {
        if (!blueprintModeActive || activeBlueprint == null) return;

        var wallToRemove = activeBlueprint.plannedWalls.Find(w => w.position == position && w.floorLevel == floor);
        if (wallToRemove != null)
            activeBlueprint.RemoveWall(wallToRemove);
    }

    /// <summary>
    /// Player locks in the blueprint. Schedule build for future date.
    /// </summary>
    public void LockBlueprint(int daysFromNow = 3)
    {
        if (!blueprintModeActive || activeBlueprint == null || activeBlueprint.plannedWalls.Count == 0)
        {
            Debug.LogWarning("[BuildingManager] Cannot lock empty blueprint.");
            return;
        }

        DateTime buildStartDate = GameTimeManager.Instance.CurrentTime.AddDays(daysFromNow);
        activeBlueprint.LockBlueprint(buildStartDate, daysPerTile: 1);

        blueprintModeActive = false;

        // Re-enable click-to-move
        ClickToMove clickToMove = FindObjectOfType<ClickToMove>();
        if (clickToMove) clickToMove.enabled = true;

        OnBlueprintLocked?.Invoke();

        Debug.Log($"[BuildingManager] Blueprint locked. Build scheduled for {buildStartDate:yyyy-MM-dd}. Duration: {activeBlueprint.daysToComplete} days.");
    }

    /// <summary>
    /// Check if any scheduled builds should start. Called by GameTimeManager on time change.
    /// </summary>
    private void CheckConstructionDates(DateTime currentTime)
    {
        if (activeBlueprint == null || !activeBlueprint.isLocked) return;

        // If current time matches or exceeds scheduled start, begin construction
        if (currentTime >= activeBlueprint.scheduledStartDate && activeBlueprint.plannedWalls[0].state == WallState.Planned)
        {
            BeginConstruction();
        }

        // Check if construction should complete
        if (currentTime >= activeBlueprint.estimatedCompletionDate &&
            activeBlueprint.plannedWalls[0].state == WallState.UnderConstruction)
        {
            CompleteConstruction();
        }
    }

    private void BeginConstruction()
    {
        if (activeBlueprint == null) return;

        // Change all walls from Planned → UnderConstruction
        foreach (var wall in activeBlueprint.plannedWalls)
        {
            wall.state = WallState.UnderConstruction;
            wall.completionDate = activeBlueprint.estimatedCompletionDate;
            wallGrid.AddWall(wall);
        }

        OnConstructionStarted?.Invoke();
        Debug.Log("[BuildingManager] Construction started. Builders have arrived.");
    }

    private void CompleteConstruction()
    {
        if (activeBlueprint == null) return;

        // Change all walls from UnderConstruction → Completed
        foreach (var wall in activeBlueprint.plannedWalls)
        {
            wall.state = WallState.Completed;
        }

        // Place wall tiles on the tilemap
        Debug.Log($"[BuildingManager] wallTilePrefab is {(wallTilePrefab == null ? "NULL" : "assigned")}");

        if (wallTilePrefab != null)
        {
            Tilemap tilemap = FloorManager.Instance.GetTilemap(0);
            Debug.Log($"[BuildingManager] Tilemap is {(tilemap == null ? "NULL" : "valid")}");
            Debug.Log($"[BuildingManager] Placing {activeBlueprint.plannedWalls.Count} wall tiles");

            foreach (var wall in activeBlueprint.plannedWalls)
            {
                Debug.Log($"[BuildingManager] SetTile at {wall.position}");
                tilemap.SetTile(wall.position, wallTilePrefab);
            }
        }

        OnConstructionCompleted?.Invoke();
        Debug.Log("[BuildingManager] Construction completed. All walls finished.");

        // Rebake pathfinding since walls are now in place
        PathfindingGrid.Instance.BakeAll();
    }

    /// <summary>
    /// Get the active blueprint for UI/preview purposes.
    /// </summary>
    public BuildingBlueprint GetActiveBlueprint() => activeBlueprint;

    public List<Wall> GetActiveBlueprintWalls() => activeBlueprint?.plannedWalls ?? new();

    public bool IsBlueprintModeActive() => blueprintModeActive;

    public WallGrid GetWallGrid() => wallGrid;

    /// <summary>
    /// Query if a specific cell position has a wall that blocks movement.
    /// </summary>
    public bool IsWallBlocking(Vector3Int position, int floor)
    {
        var wall = wallGrid.GetWall(position, floor);
        return wall != null && (wall.state == WallState.Completed || wall.state == WallState.UnderConstruction);
    }
}
