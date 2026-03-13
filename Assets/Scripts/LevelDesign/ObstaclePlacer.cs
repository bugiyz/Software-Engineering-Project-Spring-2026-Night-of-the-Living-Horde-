using System.Collections.Generic;
using UnityEngine;

public class ObstaclePlacer : MonoBehaviour
{
    public enum PrefabGroup
    {
        ObstaclesOnly,
        FoliageOnly,
        ObstaclesAndFoliage,
        StructurePropsOnly
    }

    [Header("Theme")]
    public LevelThemeData theme;
    public PrefabGroup prefabGroup = PrefabGroup.ObstaclesAndFoliage;

    [Header("Placement Area")]
    public Vector2 areaMin = new Vector2(-20f, -20f);
    public Vector2 areaMax = new Vector2(20f, 20f);
    [Min(1)] public int count = 30;
    [Min(0.1f)] public float minSpacing = 1.25f;
    [Min(1)] public int maxAttempts = 800;

    [Header("Optional Collision Check")]
    public LayerMask blockedLayers;
    [Min(0.05f)] public float overlapRadius = 0.35f;

    [Header("Parenting")]
    public Transform container;
    public bool clearExistingBeforePlace = true;

    [Header("Random")]
    public bool randomizeSeed = true;
    public int seed = 12345;

    [ContextMenu("Place Obstacles")]
    public void PlaceObstacles()
    {
        if (theme == null)
        {
            Debug.LogWarning("Missing LevelThemeData reference.", this);
            return;
        }

        GameObject[] source = GetPrefabSource();
        if (source == null || source.Length == 0)
        {
            Debug.LogWarning("Selected prefab group has no prefabs assigned.", this);
            return;
        }

        Transform root = container != null ? container : transform;
        if (clearExistingBeforePlace)
        {
            ClearChildren(root);
        }

        int runSeed = randomizeSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
        Random.InitState(runSeed);

        List<Vector2> placed = new List<Vector2>(count);
        int placedCount = 0;
        int attempts = 0;

        while (placedCount < count && attempts < maxAttempts)
        {
            attempts++;
            Vector2 candidate = new Vector2(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y));

            if (!HasSpacing(placed, candidate))
            {
                continue;
            }

            if (blockedLayers.value != 0 && Physics2D.OverlapCircle(candidate, overlapRadius, blockedLayers) != null)
            {
                continue;
            }

            GameObject prefab = source[Random.Range(0, source.Length)];
            if (prefab == null)
            {
                continue;
            }

            float z = prefab.transform.position.z;
            Instantiate(prefab, new Vector3(candidate.x, candidate.y, z), Quaternion.identity, root);
            placed.Add(candidate);
            placedCount++;
        }

        if (placedCount < count)
        {
            Debug.LogWarning($"Placed {placedCount}/{count}. Increase area or reduce spacing.", this);
        }
    }

    [ContextMenu("Clear Placed Obstacles")]
    public void ClearPlaced()
    {
        Transform root = container != null ? container : transform;
        ClearChildren(root);
    }

    private bool HasSpacing(List<Vector2> placed, Vector2 candidate)
    {
        float minDistSq = minSpacing * minSpacing;
        for (int i = 0; i < placed.Count; i++)
        {
            if ((placed[i] - candidate).sqrMagnitude < minDistSq)
            {
                return false;
            }
        }

        return true;
    }

    private GameObject[] GetPrefabSource()
    {
        switch (prefabGroup)
        {
            case PrefabGroup.ObstaclesOnly:
                return theme.obstaclePrefabs;
            case PrefabGroup.FoliageOnly:
                return theme.foliagePrefabs;
            case PrefabGroup.StructurePropsOnly:
                return theme.structurePropPrefabs;
            default:
                return Join(theme.obstaclePrefabs, theme.foliagePrefabs);
        }
    }

    private static GameObject[] Join(GameObject[] first, GameObject[] second)
    {
        int firstLen = first != null ? first.Length : 0;
        int secondLen = second != null ? second.Length : 0;
        GameObject[] result = new GameObject[firstLen + secondLen];

        for (int i = 0; i < firstLen; i++)
        {
            result[i] = first[i];
        }

        for (int i = 0; i < secondLen; i++)
        {
            result[firstLen + i] = second[i];
        }

        return result;
    }

    private void ClearChildren(Transform root)
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

    private void OnDrawGizmosSelected()
    {
        Vector3 center = new Vector3((areaMin.x + areaMax.x) * 0.5f, (areaMin.y + areaMax.y) * 0.5f, 0f);
        Vector3 size = new Vector3(Mathf.Abs(areaMax.x - areaMin.x), Mathf.Abs(areaMax.y - areaMin.y), 0f);
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.35f);
        Gizmos.DrawWireCube(center, size);
    }
}
