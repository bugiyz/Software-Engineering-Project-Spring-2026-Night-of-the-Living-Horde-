using UnityEngine;

// This script is responsible for applying damage to the player when the zombie touches them

public class ZombieDamageOnTouch : MonoBehaviour
{
    // The amount of damage to apply to the player when touched
    // Set in the inspector for easy tweaking
    public int damage = 10;
    public float damageCooldown = 1.0f;
    // A timer to track when the next damage can be applied to prevent rapid damage application
    float nextDamageTime;

    private void OnCollisionStay2D(Collision2D collision)
    {
        // If the current time is less than the next allowed damage time, return early 
        // to prevent applying damage
        if (Time.time < nextDamageTime) return;
        // Check if the collided object has the "Player" tag to ensure we only apply damage to the 
        // player
        if (collision.collider.CompareTag("Player"))
        {
            // Get the PlayerHealth component from the collided object to apply damage
            PlayerHealth ph = collision.collider.GetComponent<PlayerHealth>();
            // If the player has a PlayerHealth component, call the TakeDamage method to apply damage
            if (ph != null)
            {
                ph.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
            }
        }
    }
}
