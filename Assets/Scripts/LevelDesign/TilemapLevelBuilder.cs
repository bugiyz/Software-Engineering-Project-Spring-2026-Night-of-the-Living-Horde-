using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class TilemapLevelBuilder : MonoBehaviour
{
    public enum LayoutStyle
    {
        CaveArena,
        Cathedral
    }

    [Header("Theme")]
    public LevelThemeData theme;

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    [FormerlySerializedAs("cathedralFloorTilemap")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public bool autoCreateTilemaps = true;
    public bool buildOnStart = true;
    public LayoutStyle layoutStyle = LayoutStyle.CaveArena;

    [Header("World Area")]
    [Min(1)] public int mapWidth = 120;
    [Min(1)] public int mapHeight = 120;
    public Vector2Int mapOrigin = new Vector2Int(-60, -60);

    [Header("Cave Look")]
    public bool applyCaveTint = true;
    public Color caveFloorTint = new Color(0.87f, 0.45f, 0.12f, 1f);
    public Color caveWallTint = new Color(0.2f, 0.2f, 0.33f, 1f);
    [Min(1)] public int caveBorderThickness = 6;
    [Range(0.01f, 0.5f)] public float caveEdgeNoiseScale = 0.07f;
    [Min(0)] public int caveEdgeNoiseStrength = 4;
    [Min(0)] public int caveChunkCount = 12;
    [Min(1)] public int caveChunkMinSize = 3;
    [Min(1)] public int caveChunkMaxSize = 7;
    public int caveSeed = 1337;
    public bool useDesignedArenaCover = true;
    [Min(2)] public int combatLaneHalfWidth = 10;

    [Header("Cathedral Shape")]
    public Vector2Int cathedralCenter = new Vector2Int(0, 10);
    [Min(8)] public int naveWidth = 18;
    [Min(10)] public int naveHeight = 28;
    [Min(10)] public int transeptWidth = 34;
    [Min(6)] public int transeptHeight = 8;
    [Min(8)] public int apseWidth = 12;
    [Min(6)] public int apseHeight = 10;
    [Min(1)] public int southEntranceWidth = 3;

    [Header("Path")]
    [Min(1)] public int pathHalfWidth = 2;

    [Header("Graveyard")]
    [Min(2)] public int gravesPerSide = 24;
    [Min(1)] public int graveyardPadding = 4;
    public Transform graveyardContainer;

    [Header("World Border")]
    public bool createWorldBorder = true;

    [Header("Generated Objects")]
    public bool clearGeneratedPropsOnBuild = true;
    public Transform generatedPropsRoot;

    private readonly List<Vector3Int> floorCells = new List<Vector3Int>(2048);

    [ContextMenu("Build Level")]
    public void BuildLevel()
    {
        EnsureTilemaps();

        if (theme == null || groundTilemap == null || floorTilemap == null || wallTilemap == null)
        {
            Debug.LogWarning("Assign theme and all tilemaps before building.", this);
            return;
        }

        groundTilemap.ClearAllTiles();
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        floorCells.Clear();
        if (clearGeneratedPropsOnBuild)
        {
            ClearGeneratedProps();
        }

        if (layoutStyle == LayoutStyle.Cathedral)
        {
            groundTilemap.color = Color.white;
            floorTilemap.color = Color.white;
            wallTilemap.color = Color.white;

            PaintGrass();
            PaintCathedralFloorplan();
            PaintCathedralWalls();
            PaintPathToSouthEntrance();

            if (createWorldBorder)
            {
                PaintWorldBorder();
            }

            BuildGraveyards();
        }
        else
        {
            BuildCaveArena();
        }

        EnsureWallCollision();
    }

    private void Start()
    {
        if (buildOnStart)
        {
            BuildLevel();
        }
    }

    private void PaintGrass()
    {
        if (theme.groundTile == null)
        {
            return;
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int cell = new Vector3Int(mapOrigin.x + x, mapOrigin.y + y, 0);
                groundTilemap.SetTile(cell, theme.groundTile);
            }
        }
    }

    private void BuildCaveArena()
    {
        TileBase floorTile = theme.floorTile != null ? theme.floorTile : theme.groundTile;
        TileBase wallTile = theme.wallTile;

        if (floorTile == null || wallTile == null)
        {
            Debug.LogWarning("Cave mode requires floor and wall tiles in LevelThemeData.", this);
            return;
        }

        if (applyCaveTint)
        {
            groundTilemap.color = caveFloorTint;
            floorTilemap.color = caveFloorTint;
            wallTilemap.color = caveWallTint;
        }
        else
        {
            groundTilemap.color = Color.white;
            floorTilemap.color = Color.white;
            wallTilemap.color = Color.white;
        }

        FillMap(floorTilemap, floorTile);
        PaintNoisyBorderWalls(wallTile);

        if (useDesignedArenaCover)
        {
            PaintArenaCoverBlocks(wallTile);
        }
        else
        {
            PaintInteriorChunks(wallTile);
        }
    }

    private void FillMap(Tilemap map, TileBase tile)
    {
        int xMin = mapOrigin.x;
        int yMin = mapOrigin.y;
        int xMax = mapOrigin.x + mapWidth - 1;
        int yMax = mapOrigin.y + mapHeight - 1;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                map.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private void PaintNoisyBorderWalls(TileBase wallTile)
    {
        int xMin = mapOrigin.x;
        int yMin = mapOrigin.y;
        int xMax = mapOrigin.x + mapWidth - 1;
        int yMax = mapOrigin.y + mapHeight - 1;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                int lx = x - xMin;
                int ly = y - yMin;
                int rx = xMax - x;
                int ty = yMax - y;

                int leftNoise = NoiseOffset(y, 11f);
                int rightNoise = NoiseOffset(y, 29f);
                int bottomNoise = NoiseOffset(x, 47f);
                int topNoise = NoiseOffset(x, 83f);

                bool inLeftWall = lx <= caveBorderThickness + leftNoise;
                bool inRightWall = rx <= caveBorderThickness + rightNoise;
                bool inBottomWall = ly <= caveBorderThickness + bottomNoise;
                bool inTopWall = ty <= caveBorderThickness + topNoise;

                if (inLeftWall || inRightWall || inBottomWall || inTopWall)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }
    }

    private int NoiseOffset(int axis, float salt)
    {
        float n = Mathf.PerlinNoise((axis + caveSeed) * caveEdgeNoiseScale, salt + caveSeed * 0.001f);
        return Mathf.RoundToInt((n - 0.5f) * 2f * caveEdgeNoiseStrength);
    }

    private void PaintInteriorChunks(TileBase wallTile)
    {
        Random.InitState(caveSeed);

        int xMin = mapOrigin.x + caveBorderThickness + caveEdgeNoiseStrength + 2;
        int yMin = mapOrigin.y + caveBorderThickness + caveEdgeNoiseStrength + 2;
        int xMax = mapOrigin.x + mapWidth - caveBorderThickness - caveEdgeNoiseStrength - 3;
        int yMax = mapOrigin.y + mapHeight - caveBorderThickness - caveEdgeNoiseStrength - 3;

        for (int i = 0; i < caveChunkCount; i++)
        {
            int cx = Random.Range(xMin, xMax);
            int cy = Random.Range(yMin, yMax);
            int halfW = Random.Range(caveChunkMinSize, caveChunkMaxSize + 1);
            int halfH = Random.Range(caveChunkMinSize, caveChunkMaxSize + 1);

            if (Mathf.Abs(cx - cathedralCenter.x) < 8 && Mathf.Abs(cy - cathedralCenter.y) < 8)
            {
                continue;
            }

            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                for (int y = cy - halfH; y <= cy + halfH; y++)
                {
                    float dx = (x - cx) / (float)halfW;
                    float dy = (y - cy) / (float)halfH;
                    float d = dx * dx + dy * dy;
                    if (d > 1.15f)
                    {
                        continue;
                    }

                    wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }
    }

    private void PaintArenaCoverBlocks(TileBase wallTile)
    {
        int xMin = mapOrigin.x;
        int yMin = mapOrigin.y;
        int xMax = mapOrigin.x + mapWidth - 1;
        int centerX = mapOrigin.x + mapWidth / 2;
        int centerY = mapOrigin.y + mapHeight / 2;

        // Left cave mass similar to your reference silhouette.
        PlaceWallRect(xMin + 5, yMin + 18, 12, 26, wallTile);
        PlaceWallRect(xMin + 13, yMin + 12, 10, 12, wallTile);
        PlaceWallRect(xMin + 20, yMin + 5, 26, 10, wallTile);

        // Right-side medium block and side anchors.
        PlaceWallRect(xMax - 22, centerY - 4, 7, 9, wallTile);
        PlaceWallRect(xMax - 8, centerY + 20, 5, 10, wallTile);
        PlaceWallRect(xMax - 8, yMin + 12, 5, 10, wallTile);

        // Small mid covers/pillars for gunfights.
        PlaceWallRect(centerX - 20, centerY + 10, 5, 2, wallTile);
        PlaceWallRect(centerX - 14, centerY - 6, 5, 2, wallTile);
        PlaceWallRect(centerX - 6, centerY - 8, 5, 2, wallTile);
        PlaceWallRect(centerX + 8, centerY + 8, 4, 2, wallTile);
        PlaceWallRect(centerX + 18, centerY - 2, 4, 2, wallTile);

        // Keep central combat lane readable.
        ClearRect(centerX - combatLaneHalfWidth, yMin + 10, combatLaneHalfWidth * 2 + 1, mapHeight - 20);

        // Keep a small player start pocket open.
        ClearRect(centerX - 4, centerY - 4, 9, 9);
    }

    private void PlaceWallRect(int x, int y, int width, int height, TileBase wallTile)
    {
        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                wallTilemap.SetTile(new Vector3Int(px, py, 0), wallTile);
            }
        }
    }

    private void ClearRect(int x, int y, int width, int height)
    {
        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                wallTilemap.SetTile(new Vector3Int(px, py, 0), null);
            }
        }
    }

    private void PaintCathedralFloorplan()
    {
        if (theme.floorTile == null)
        {
            return;
        }

        Vector2Int naveOrigin = new Vector2Int(
            cathedralCenter.x - naveWidth / 2,
            cathedralCenter.y - naveHeight / 2);

        FillRectToFloor(naveOrigin, naveWidth, naveHeight);

        Vector2Int transeptOrigin = new Vector2Int(
            cathedralCenter.x - transeptWidth / 2,
            cathedralCenter.y - transeptHeight / 2);
        FillRectToFloor(transeptOrigin, transeptWidth, transeptHeight);

        Vector2Int apseOrigin = new Vector2Int(
            cathedralCenter.x - apseWidth / 2,
            naveOrigin.y + naveHeight - 1);
        FillRectToFloor(apseOrigin, apseWidth, apseHeight);

        for (int i = 0; i < floorCells.Count; i++)
        {
            floorTilemap.SetTile(floorCells[i], theme.floorTile);
        }
    }

    private void FillRectToFloor(Vector2Int origin, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(origin.x + x, origin.y + y, 0);
                if (!floorCells.Contains(cell))
                {
                    floorCells.Add(cell);
                }
            }
        }
    }

    private void PaintCathedralWalls()
    {
        if (theme.wallTile == null)
        {
            return;
        }

        HashSet<Vector3Int> floorSet = new HashSet<Vector3Int>(floorCells);
        int entranceCenterX = cathedralCenter.x;
        int entranceMinX = entranceCenterX - southEntranceWidth / 2;
        int entranceMaxX = entranceMinX + southEntranceWidth - 1;
        int southFloorY = GetSouthMostFloorY(floorCells);

        for (int i = 0; i < floorCells.Count; i++)
        {
            Vector3Int cell = floorCells[i];
            bool isEdge = !floorSet.Contains(cell + Vector3Int.left) ||
                          !floorSet.Contains(cell + Vector3Int.right) ||
                          !floorSet.Contains(cell + Vector3Int.up) ||
                          !floorSet.Contains(cell + Vector3Int.down);

            if (!isEdge)
            {
                continue;
            }

            bool southEntranceOpening =
                cell.y == southFloorY &&
                cell.x >= entranceMinX &&
                cell.x <= entranceMaxX;

            if (!southEntranceOpening)
            {
                wallTilemap.SetTile(cell, theme.wallTile);
            }
        }
    }

    private void PaintPathToSouthEntrance()
    {
        if (theme.pathTile == null)
        {
            return;
        }

        int entranceY = GetSouthMostFloorY(floorCells);
        int pathStartY = mapOrigin.y + 1;

        for (int y = pathStartY; y <= entranceY; y++)
        {
            for (int x = cathedralCenter.x - pathHalfWidth; x <= cathedralCenter.x + pathHalfWidth; x++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), theme.pathTile);
            }
        }
    }

    private void PaintWorldBorder()
    {
        if (theme.wallTile == null)
        {
            return;
        }

        int xMin = mapOrigin.x;
        int yMin = mapOrigin.y;
        int xMax = mapOrigin.x + mapWidth - 1;
        int yMax = mapOrigin.y + mapHeight - 1;

        for (int x = xMin; x <= xMax; x++)
        {
            wallTilemap.SetTile(new Vector3Int(x, yMin, 0), theme.wallTile);
            wallTilemap.SetTile(new Vector3Int(x, yMax, 0), theme.wallTile);
        }

        for (int y = yMin; y <= yMax; y++)
        {
            wallTilemap.SetTile(new Vector3Int(xMin, y, 0), theme.wallTile);
            wallTilemap.SetTile(new Vector3Int(xMax, y, 0), theme.wallTile);
        }
    }

    private void BuildGraveyards()
    {
        GameObject[] graves = GetGravePrefabs();
        if (graves.Length == 0)
        {
            return;
        }

        Transform root = EnsureGraveyardContainer();
        ClearChildren(root);

        GetFloorBounds(floorCells, out int minX, out int maxX, out int minY, out int maxY);

        RectInt left = new RectInt(
            minX - graveyardPadding - 10,
            minY - 2,
            8,
            (maxY - minY) + 5);
        RectInt right = new RectInt(
            maxX + graveyardPadding + 2,
            minY - 2,
            8,
            (maxY - minY) + 5);

        PlaceGravesInRect(graves, root, left, gravesPerSide);
        PlaceGravesInRect(graves, root, right, gravesPerSide);
    }

    private void PlaceGravesInRect(GameObject[] prefabs, Transform parent, RectInt area, int count)
    {
        int columns = Mathf.Max(2, Mathf.FloorToInt(Mathf.Sqrt(count)));
        int rows = Mathf.CeilToInt((float)count / columns);

        float cellWidth = area.width / (float)columns;
        float cellHeight = area.height / (float)rows;
        int placed = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (placed >= count)
                {
                    return;
                }

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                if (prefab == null)
                {
                    continue;
                }

                float px = area.xMin + (col + 0.5f) * cellWidth + Random.Range(-0.25f, 0.25f);
                float py = area.yMin + (row + 0.5f) * cellHeight + Random.Range(-0.25f, 0.25f);
                float angle = Random.Range(-8f, 8f);
                float z = prefab.transform.position.z;

                Instantiate(prefab, new Vector3(px, py, z), Quaternion.Euler(0f, 0f, angle), parent);
                placed++;
            }
        }
    }

    private GameObject[] GetGravePrefabs()
    {
        List<GameObject> graves = new List<GameObject>();
        AddMatchingPrefabs(graves, theme.structurePropPrefabs);
        AddMatchingPrefabs(graves, theme.obstaclePrefabs);
        return graves.ToArray();
    }

    private static void AddMatchingPrefabs(List<GameObject> target, GameObject[] source)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Length; i++)
        {
            GameObject prefab = source[i];
            if (prefab == null)
            {
                continue;
            }

            string n = prefab.name.ToLowerInvariant();
            if (n.Contains("grave") || n.Contains("coffin") || n.Contains("stone"))
            {
                target.Add(prefab);
            }
        }
    }

    private Transform EnsureGraveyardContainer()
    {
        Transform parent = EnsureGeneratedPropsRoot();

        if (graveyardContainer != null)
        {
            return graveyardContainer;
        }

        Transform existing = parent.Find("GraveyardProps");
        if (existing != null)
        {
            graveyardContainer = existing;
            return graveyardContainer;
        }

        GameObject go = new GameObject("GraveyardProps");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        graveyardContainer = go.transform;
        return graveyardContainer;
    }

    [ContextMenu("Clear Generated Props")]
    public void ClearGeneratedProps()
    {
        Transform root = EnsureGeneratedPropsRoot();
        if (root != null)
        {
            ClearChildren(root);
        }

        graveyardContainer = null;
    }

    private Transform EnsureGeneratedPropsRoot()
    {
        if (generatedPropsRoot != null)
        {
            return generatedPropsRoot;
        }

        Transform existing = transform.Find("GeneratedProps");
        if (existing != null)
        {
            generatedPropsRoot = existing;
            return generatedPropsRoot;
        }

        GameObject go = new GameObject("GeneratedProps");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        generatedPropsRoot = go.transform;
        return generatedPropsRoot;
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private static int GetSouthMostFloorY(List<Vector3Int> cells)
    {
        int minY = int.MaxValue;
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].y < minY)
            {
                minY = cells[i].y;
            }
        }

        return minY;
    }

    private static void GetFloorBounds(List<Vector3Int> cells, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        maxY = int.MinValue;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int c = cells[i];
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }
    }

    private void EnsureTilemaps()
    {
        if (!autoCreateTilemaps)
        {
            return;
        }

        Grid grid = GetComponentInParent<Grid>();
        if (grid == null)
        {
            grid = Object.FindFirstObjectByType<Grid>();
        }

        if (grid == null)
        {
            GameObject gridGo = new GameObject("Grid", typeof(Grid));
            grid = gridGo.GetComponent<Grid>();
        }

        if (groundTilemap == null)
        {
            groundTilemap = CreateTilemapUnderGrid(grid.transform, "Ground", 0);
        }

        if (floorTilemap == null)
        {
            floorTilemap = CreateTilemapUnderGrid(grid.transform, "Floor", 1);
        }

        if (wallTilemap == null)
        {
            wallTilemap = CreateTilemapUnderGrid(grid.transform, "WallsAndBorder", 2);
        }
    }

    private static Tilemap CreateTilemapUnderGrid(Transform grid, string name, int sortingOrder)
    {
        Transform existing = grid.Find(name);
        GameObject tilemapGo;
        if (existing != null)
        {
            tilemapGo = existing.gameObject;
            if (tilemapGo.GetComponent<Tilemap>() != null)
            {
                TilemapRenderer existingRenderer = tilemapGo.GetComponent<TilemapRenderer>();
                if (existingRenderer != null)
                {
                    existingRenderer.sortingOrder = sortingOrder;
                }

                return tilemapGo.GetComponent<Tilemap>();
            }
        }
        else
        {
            tilemapGo = new GameObject(name);
            tilemapGo.transform.SetParent(grid);
            tilemapGo.transform.localPosition = Vector3.zero;
        }

        Tilemap tilemap = tilemapGo.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            tilemap = tilemapGo.AddComponent<Tilemap>();
        }

        TilemapRenderer renderer = tilemapGo.GetComponent<TilemapRenderer>();
        if (renderer == null)
        {
            renderer = tilemapGo.AddComponent<TilemapRenderer>();
        }

        renderer.sortingOrder = sortingOrder;
        return tilemap;
    }

    private void EnsureWallCollision()
    {
        GameObject wallGo = wallTilemap.gameObject;

        Rigidbody2D rb = wallGo.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = wallGo.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D composite = wallGo.GetComponent<CompositeCollider2D>();
        if (composite == null)
        {
            composite = wallGo.AddComponent<CompositeCollider2D>();
        }

        TilemapCollider2D tilemapCollider = wallGo.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            tilemapCollider = wallGo.AddComponent<TilemapCollider2D>();
        }

        tilemapCollider.usedByComposite = true;
    }
}
