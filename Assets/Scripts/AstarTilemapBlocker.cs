using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

// Marks all occupied cells in a wall tilemap as unwalkable in the A* grid graph.
public class AstarTilemapBlocker : MonoBehaviour
{
    // Creates a new instance of the AstarTilemapBlocker.
    [Header("References")]
    public Tilemap wallTilemap;

    // Applies the wall tiles to the A* graph.
    [Header("Settings")]
    public bool rescanOnStart = true;
    public bool applyWallsOnStart = true;

    void Start()
    {
        // Check if the wall tilemap is assigned.
        if (wallTilemap == null)
        {
            Debug.LogWarning("AstarTilemapBlocker: No wall tilemap assigned.");
            return;
        }

        // Rescan the graph first so we start with a fresh grid.
        if (rescanOnStart && AstarPath.active != null)
        {
            AstarPath.active.Scan();
        }

        // Then mark wall tiles as blocked.
        if (applyWallsOnStart)
        {
            ApplyWallTilesToGraph();
        }
    }

    // Applies the wall tiles to the A* graph.
    [ContextMenu("Apply Wall Tiles To Graph")]

    // function to apply wall tiles to the A* graph.
    public void ApplyWallTilesToGraph()
    {
        // Check if the A* path is active.
        if (AstarPath.active == null)
        {
            Debug.LogWarning("AstarTilemapBlocker: No active AstarPath found.");
            return;
        }
        // Check if the wall tilemap is assigned.
        if (wallTilemap == null)
        {
            Debug.LogWarning("AstarTilemapBlocker: No wall tilemap assigned.");
            return;
        }

        // Get the bounds of the wall tilemap.
        BoundsInt bounds = wallTilemap.cellBounds;

        // Iterate through all cells in the bounds.
        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            // Skip empty cells.
            if (!wallTilemap.HasTile(cellPos))
                continue;

            // Get the center of the tile in world space.
            Vector3 worldCenter = wallTilemap.GetCellCenterWorld(cellPos);

            // Use the tile's cell size as the blocked area.
            Vector3 cellSize = wallTilemap.cellSize;

            // Create a bounds object for the tile.
            Bounds tileBounds = new Bounds(worldCenter, new Vector3(cellSize.x, cellSize.y, 1f));

            // Update the A* graph with the blocked area.
            GraphUpdateObject guo = new GraphUpdateObject(tileBounds);
            guo.modifyWalkability = true;
            guo.setWalkability = false;

            // Apply the graph update.
            AstarPath.active.UpdateGraphs(guo);
        }

        // Make sure all queued graph updates are applied immediately.
        AstarPath.active.FlushGraphUpdates();

        // Debug log to confirm the operation was a success.
        Debug.Log("AstarTilemapBlocker: Wall tiles applied to A* graph.");
    }
}