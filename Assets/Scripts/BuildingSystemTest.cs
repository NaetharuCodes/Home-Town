using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingSystemTest : MonoBehaviour
{
    private void Update()
    {
        // Press B to start blueprint mode
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("=== STARTING BLUEPRINT MODE ===");
            BuildingManager.Instance.StartBlueprintMode();
        }

        // Press W to add a test wall at mouse position
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0f));
            mouseWorld.z = 0f;
            Vector3Int cell = FloorManager.Instance.GetTilemap(0).WorldToCell(mouseWorld);

            BuildingManager.Instance.AddWallToBlueprint(cell, floor: 0,
                WallMaterial.Brick, TileType.Grass, TileType.Concrete);
            Debug.Log($"Added wall at {cell}");
        }

        // Press L to lock blueprint (schedule build for 1 day from now)
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("=== LOCKING BLUEPRINT ===");
            BuildingManager.Instance.LockBlueprint(daysFromNow: 1);
        }

        // Press T to advance game time by 2 days (to trigger construction)
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            GameTimeManager.Instance.AdvanceDays(2);
            Debug.Log($"Game time is now: {GameTimeManager.Instance.GetTimeString()}");
        }

        // Press R to print current wall grid status
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            WallGrid grid = BuildingManager.Instance.GetWallGrid();
            int wallCount = grid.GetWallCount(0);
            Debug.Log($"Floor 0 has {wallCount} walls");

            var plannedWalls = grid.GetWallsInState(WallState.Planned, 0);
            var constructionWalls = grid.GetWallsInState(WallState.UnderConstruction, 0);
            var completedWalls = grid.GetWallsInState(WallState.Completed, 0);

            Debug.Log($"  Planned: {plannedWalls.Count}, UnderConstruction: {constructionWalls.Count}, Completed: {completedWalls.Count}");
        }
    }
}

