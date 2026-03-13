using UnityEngine;

public class LevelDesignWorkbench : MonoBehaviour
{
    [Header("References")]
    public LevelThemeData theme;
    public CainosThemeAutoFill themeAutoFill;
    public TilemapLevelBuilder levelBuilder;
    public ObstaclePlacer obstaclePlacer;

    [ContextMenu("1) Auto Fill Theme")]
    public void AutoFillTheme()
    {
        if (themeAutoFill == null)
        {
            themeAutoFill = GetComponent<CainosThemeAutoFill>();
        }

        if (themeAutoFill == null)
        {
            Debug.LogWarning("Missing CainosThemeAutoFill reference.", this);
            return;
        }

#if UNITY_EDITOR
        themeAutoFill.theme = theme;
        themeAutoFill.AutoFillThemeFromCainos();
#else
        Debug.LogWarning("Theme auto-fill is editor-only.", this);
#endif
    }

    [ContextMenu("2) Build Level")]
    public void BuildLevel()
    {
        if (levelBuilder == null)
        {
            levelBuilder = GetComponent<TilemapLevelBuilder>();
        }

        if (levelBuilder == null)
        {
            Debug.LogWarning("Missing TilemapLevelBuilder reference.", this);
            return;
        }

        levelBuilder.theme = theme;
        levelBuilder.BuildLevel();
    }

    [ContextMenu("3) Build + Place Set Dressing")]
    public void BuildAndPlaceSetDressing()
    {
        BuildLevel();

        if (obstaclePlacer == null)
        {
            return;
        }

        obstaclePlacer.theme = theme;
        if (levelBuilder != null && levelBuilder.generatedPropsRoot != null)
        {
            obstaclePlacer.container = levelBuilder.generatedPropsRoot;
        }

        obstaclePlacer.PlaceObstacles();
    }

    [ContextMenu("Clear Generated")]
    public void ClearGenerated()
    {
        if (levelBuilder != null)
        {
            levelBuilder.ClearGeneratedProps();
        }

        if (obstaclePlacer != null)
        {
            obstaclePlacer.ClearPlaced();
        }
    }
}
