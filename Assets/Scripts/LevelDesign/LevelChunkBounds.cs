using UnityEngine;

public class LevelChunkBounds : MonoBehaviour
{
    public string chunkId = "Chunk_01";
    public Vector2 size = new Vector2(12f, 12f);
    public bool drawWhenNotSelected = true;
    public Color gizmoColor = new Color(0.95f, 0.75f, 0.2f, 0.4f);

    public bool ContainsWorldPoint(Vector2 point)
    {
        Vector2 center = transform.position;
        Vector2 half = size * 0.5f;
        return point.x >= center.x - half.x &&
               point.x <= center.x + half.x &&
               point.y >= center.y - half.y &&
               point.y <= center.y + half.y;
    }

    public Vector2 RandomPointInside()
    {
        Vector2 center = transform.position;
        Vector2 half = size * 0.5f;
        float x = Random.Range(center.x - half.x, center.x + half.x);
        float y = Random.Range(center.y - half.y, center.y + half.y);
        return new Vector2(x, y);
    }

    private void OnDrawGizmos()
    {
        if (!drawWhenNotSelected)
        {
            return;
        }

        DrawGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
    }
}
