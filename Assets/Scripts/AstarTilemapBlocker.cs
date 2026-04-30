using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

// Marks all occupied cells in a wall tilemap as unwalkable in the A* grid graph.
public class AstarTilemapBlocker : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;

    [Header("Settings")]
    public bool rescanOnStart = true;
    public bool applyWallsOnStart = true;

    void Start()
    {
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

    [ContextMenu("Apply Wall Tiles To Graph")]
    public void ApplyWallTilesToGraph()
    {
        if (AstarPath.active == null)
        {
            Debug.LogWarning("AstarTilemapBlocker: No active AstarPath found.");
            return;
        }

        if (wallTilemap == null)
        {
            Debug.LogWarning("AstarTilemapBlocker: No wall tilemap assigned.");
            return;
        }

        BoundsInt bounds = wallTilemap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            // Skip empty cells.
            if (!wallTilemap.HasTile(cellPos))
                continue;

            // Get the center of the tile in world space.
            Vector3 worldCenter = wallTilemap.GetCellCenterWorld(cellPos);

            // Use the tile's cell size as the blocked area.
            Vector3 cellSize = wallTilemap.cellSize;

            Bounds tileBounds = new Bounds(worldCenter, new Vector3(cellSize.x, cellSize.y, 1f));

            GraphUpdateObject guo = new GraphUpdateObject(tileBounds);
            guo.modifyWalkability = true;
            guo.setWalkability = false;

            AstarPath.active.UpdateGraphs(guo);
        }

        // Make sure all queued graph updates are applied immediately.
        AstarPath.active.FlushGraphUpdates();

        Debug.Log("AstarTilemapBlocker: Wall tiles applied to A* graph.");
    }
}