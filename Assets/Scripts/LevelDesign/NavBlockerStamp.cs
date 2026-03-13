using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NavBlockerStamp : MonoBehaviour
{
    public Vector2 size = new Vector2(1f, 1f);
    public Vector2 offset = Vector2.zero;
    public bool useAsTrigger = false;

    [ContextMenu("Apply Collider Settings")]
    public void Apply()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            return;
        }

        col.size = size;
        col.offset = offset;
        col.isTrigger = useAsTrigger;
    }

    private void Reset()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }
}
