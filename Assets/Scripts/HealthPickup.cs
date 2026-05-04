using UnityEngine;
using System.Collections;

public class HealthPickup : MonoBehaviour
{
    [Header("Heal")]
    public int healAmount = 25;

    [Header("Lifetime")]
    public float lifetime = 10f;
    public float blinkStartTime = 8f; // starts blinking at 8s

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(DespawnRoutine());
    }

    IEnumerator DespawnRoutine()
    {
        // Wait until blinking should start
        yield return new WaitForSeconds(blinkStartTime);

        float blinkDuration = lifetime - blinkStartTime;
        float timer = 0f;
        bool visible = true;

        // Blink until time runs out
        while (timer < blinkDuration)
        {
            visible = !visible;

            if (sr != null)
                sr.enabled = visible;

            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}