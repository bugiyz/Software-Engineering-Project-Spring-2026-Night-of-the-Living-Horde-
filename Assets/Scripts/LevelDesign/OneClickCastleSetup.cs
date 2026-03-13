using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class OneClickCastleSetup : MonoBehaviour
{
    [Header("Optional References")]
    public LevelThemeData theme;
    public TilemapLevelBuilder builder;
    public CainosThemeAutoFill autoFill;

    [Header("Theme Asset")]
    public string themeAssetPath = "Assets/Scripts/LevelDesign/Generated/AutoLevelThemeData.asset";

    [ContextMenu("One Click Level Setup")]
    public void SetupLevel()
    {
        SetupCastle();
    }

    [ContextMenu("One Click Cave Setup")]
    public void SetupCastle()
    {
#if UNITY_EDITOR
        EnsureThemeAsset();
        EnsureComponents();

        autoFill.theme = theme;
        autoFill.AutoFillThemeFromCainos();

        builder.theme = theme;
        builder.autoCreateTilemaps = true;
        builder.buildOnStart = true;
        builder.layoutStyle = TilemapLevelBuilder.LayoutStyle.CaveArena;
        builder.mapWidth = 140;
        builder.mapHeight = 140;
        builder.mapOrigin = new Vector2Int(-70, -70);
        builder.caveSeed = 2026;
        builder.caveBorderThickness = 8;
        builder.caveEdgeNoiseScale = 0.075f;
        builder.caveEdgeNoiseStrength = 5;
        builder.caveChunkCount = 18;
        builder.caveChunkMinSize = 2;
        builder.caveChunkMaxSize = 7;
        builder.useDesignedArenaCover = true;
        builder.combatLaneHalfWidth = 11;
        builder.applyCaveTint = true;
        builder.caveFloorTint = new Color(0.88f, 0.43f, 0.12f, 1f);
        builder.caveWallTint = new Color(0.18f, 0.2f, 0.34f, 1f);
        builder.createWorldBorder = true;
        builder.BuildLevel();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(builder);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("One-click cave arena setup complete.", this);
#else
        Debug.LogWarning("OneClickCastleSetup is editor-only.", this);
#endif
    }

#if UNITY_EDITOR
    private void EnsureThemeAsset()
    {
        if (theme != null)
        {
            return;
        }

        theme = AssetDatabase.LoadAssetAtPath<LevelThemeData>(themeAssetPath);
        if (theme != null)
        {
            return;
        }

        string folder = Path.GetDirectoryName(themeAssetPath);
        if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        theme = ScriptableObject.CreateInstance<LevelThemeData>();
        AssetDatabase.CreateAsset(theme, themeAssetPath);
        AssetDatabase.SaveAssets();
    }

    private void EnsureComponents()
    {
        if (builder == null)
        {
            builder = GetComponent<TilemapLevelBuilder>();
            if (builder == null)
            {
                builder = gameObject.AddComponent<TilemapLevelBuilder>();
            }
        }

        if (autoFill == null)
        {
            autoFill = GetComponent<CainosThemeAutoFill>();
            if (autoFill == null)
            {
                autoFill = gameObject.AddComponent<CainosThemeAutoFill>();
            }
        }
    }
#endif
}
