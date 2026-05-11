using UnityEngine;
using System.Collections;

// This script is responsible for handling health pickups in the game.

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
        // Get the SpriteRenderer component.
        sr = GetComponent<SpriteRenderer>();
        // Start the despawn routine after the lifetime has passed.
        StartCoroutine(DespawnRoutine());
    }

    IEnumerator DespawnRoutine()
    {
        // Wait until blinking should start
        yield return new WaitForSeconds(blinkStartTime);
        // Start blinking
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
    // function to handle collision with the player
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that triggered the collision is the player.
        if (!other.CompareTag("Player")) return;
        // Get the PlayerHealth component from the player object.
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        // If the player is found, heal them.
        if (player != null)
        {
            player.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}