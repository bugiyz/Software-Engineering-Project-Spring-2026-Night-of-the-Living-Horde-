using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Tilemaps;
#endif

public class CainosThemeAutoFill : MonoBehaviour
{
    public LevelThemeData theme;

#if UNITY_EDITOR
    private const string CainosRoot = "Assets/Cainos/Pixel Art Top Down - Basic";

    [ContextMenu("Auto Fill Theme From Cainos")]
    public void AutoFillThemeFromCainos()
    {
        if (theme == null)
        {
            Debug.LogWarning("Assign a LevelThemeData asset first.", this);
            return;
        }

        theme.groundTile = LoadTile($"{CainosRoot}/Tile Palette/TP Grass/TX Tileset Grass 0.asset");
        theme.pathTile = LoadTile($"{CainosRoot}/Tile Palette/TP Grass/TX Tileset Grass Pavement 0.asset");
        theme.floorTile = LoadTile($"{CainosRoot}/Tile Palette/TP Stone Ground/TX Tileset Stone Ground_0.asset");
        theme.wallTile = LoadTile($"{CainosRoot}/Tile Palette/TP Wall/TX Tileset Wall_0.asset");

        theme.obstaclePrefabs = FindPrefabs("Assets/Cainos/Pixel Art Top Down - Basic/Prefab/Props", new[]
        {
            "Stone", "Crate", "Barrel", "Wooden Gate", "Brick", "Pillar"
        });

        theme.structurePropPrefabs = FindPrefabs("Assets/Cainos/Pixel Art Top Down - Basic/Prefab/Props", new[]
        {
            "Gate", "Stairs", "Coffin", "Bench", "Rune", "Altar", "Statue", "Lantern"
        });

        theme.foliagePrefabs = FindPrefabs("Assets/Cainos/Pixel Art Top Down - Basic/Prefab/Plant", new[]
        {
            "Grass", "Bush", "Flower", "Tree"
        });

        EditorUtility.SetDirty(theme);
        AssetDatabase.SaveAssets();
        Debug.Log("Cainos theme auto-fill complete.", theme);
    }

    private static TileBase LoadTile(string path)
    {
        return AssetDatabase.LoadAssetAtPath<TileBase>(path);
    }

    private static GameObject[] FindPrefabs(string folder, string[] includeWords)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        List<GameObject> list = new List<GameObject>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!ContainsAny(fileName, includeWords))
            {
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                list.Add(prefab);
            }
        }

        return list.ToArray();
    }

    private static bool ContainsAny(string value, string[] words)
    {
        for (int i = 0; i < words.Length; i++)
        {
            if (value.IndexOf(words[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
#endif
}
