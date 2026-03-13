using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class PlayerAlwaysVisible : MonoBehaviour
{
    public string sortingLayerName = "Default";
    public int sortingOrder = 500;
    public bool applyToChildSpriteRenderers = true;

    private SortingGroup sortingGroup;

    private void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
        ApplySorting();
    }

    private void LateUpdate()
    {
        // Keep this stable even if animations/prefabs override renderer orders.
        ApplySorting();
    }

    [ContextMenu("Apply Sorting Now")]
    public void ApplySorting()
    {
        if (sortingGroup == null)
        {
            sortingGroup = GetComponent<SortingGroup>();
            if (sortingGroup == null)
            {
                return;
            }
        }

        sortingGroup.sortingLayerName = sortingLayerName;
        sortingGroup.sortingOrder = sortingOrder;

        if (!applyToChildSpriteRenderers)
        {
            return;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingLayerName = sortingLayerName;
            renderers[i].sortingOrder = sortingOrder;
        }
    }
}
