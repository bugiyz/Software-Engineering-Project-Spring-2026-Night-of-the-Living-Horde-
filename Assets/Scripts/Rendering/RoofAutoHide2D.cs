using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoofAutoHide2D : MonoBehaviour
{
    public string playerTag = "Player";
    public Renderer[] roofRenderers;
    public bool disableEntireRoofObject = false;

    private int playerInsideCount;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Awake()
    {
        if (roofRenderers == null || roofRenderers.Length == 0)
        {
            roofRenderers = GetComponentsInChildren<Renderer>(true);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInsideCount++;
        SetRoofVisible(false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        if (playerInsideCount == 0)
        {
            SetRoofVisible(true);
        }
    }

    private void SetRoofVisible(bool visible)
    {
        if (disableEntireRoofObject)
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(visible);
                }
            }

            return;
        }

        for (int i = 0; i < roofRenderers.Length; i++)
        {
            if (roofRenderers[i] != null)
            {
                roofRenderers[i].enabled = visible;
            }
        }
    }
}
