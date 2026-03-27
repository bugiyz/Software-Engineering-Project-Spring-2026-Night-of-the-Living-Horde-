using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
// This script is responsible for managing the zombie's health, including taking damage, 
// updating the health bar UI, and handling death behavior such as playing death animations 
// and sounds, disabling AI, and destroying the zombie object after death.
public class ZombieHealth : MonoBehaviour
{
    // Creating an event to notify other scripts when the zombie dies
    // This allows for communication with other systems like zombie spawners
    // to remove the current zombie from their tracking lists when it dies
    public event Action OnZombieDeath;
    // The maximum health of the zombie, set in the inspector for easy tweaking
    [Header("Health")]
    public int maxHealth = 50;
    public int currentHealth;

    [Header("UI (assign in Inspector)")]
    public Image healthBarFill;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hurtClip;
    public AudioClip deathClip;

    void Start()
    {
        // Initialize the zombie's current health to the maximum health at the start of the game
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        // If the zombie is already dead return early to prevent applying damage or playing hurt 
        // sounds
        if (currentHealth <= 0) return;
        // Reduce the current health by the damage amount and clamp it to a minimum of 0
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        // Update the health bar UI to reflect the new health value
        UpdateBar();

        // Play Hurt Sound
        audioSource.PlayOneShot(hurtClip);
        // If health has reached 0, call the Die method to handle death behavior
        if (currentHealth == 0)
            Die();
            
    }

    void UpdateBar()
    {
        // If the health bar fill image is assigned
        // Update the fill amount based on the current health percentage
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
{
    // Play Death Sound
    if (audioSource != null && deathClip != null)
        audioSource.PlayOneShot(deathClip);
        // Trigger the death animation by setting the "Die" trigger in the Animator
        Animator anim = GetComponentInChildren<Animator>();
        // If the Animator component is found, set the "Die" trigger to play the death animation
        if (anim != null)
            anim.SetTrigger("Die");
        // Disable the ZombieAI script to stop all movement and behavior immediately upon death
        ZombieAI ai = GetComponent<ZombieAI>();
        if (ai != null)
            ai.enabled = false;
        // Start the death routine coroutine to handle disabling colliders, stopping movement,
        StartCoroutine(DeathRoutine());
}

    IEnumerator DeathRoutine()
{
    // Create references to the collider and rigidbody components to disable them after death
    Collider2D col = GetComponent<Collider2D>();
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    // Disable the collider to prevent further collisions with the player or other objects after death
    if (col != null)
        col.enabled = false;
    // Stop all movement by setting velocity to zero and disabling the rigidbody simulation
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.simulated = false;
    }

    // wait for death animation to finish
    float waitTime = 0.5f;

    // Keep sprite visible while death sound plays
    if (deathClip != null)
        waitTime = Mathf.Max(waitTime, deathClip.length);
    // Wait for the specified time to allow the death animation and sound to play
    yield return new WaitForSeconds(waitTime);
    // Invoke the onZombieDeath event to notify any scripts that the zombie has died
    // allowing for cleanup in systems such as zombie spawners that are tracking this zombie
    OnZombieDeath?.Invoke();
    // Destroy the zombie game object to remove it from the scene after death
    Destroy(gameObject);
}
}