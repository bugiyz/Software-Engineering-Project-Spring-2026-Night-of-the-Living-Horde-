using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
// This script is responsible for managing the player's health, including taking damage, healing, 
// updating the health UI, and handling death behavior such as playing death animations, disabling
// player controls, and returning to the main menu after death.

public class PlayerHealth : MonoBehaviour
{
    // All variables are public for display in inspector for easy tweaking without code changes
    // Except private variables that are only used internally
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    [Header("Invincibility")]
    public float invincibleTime = 1f;
    private float lastDamageTime;

    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        // Initialize the player's current health to the maximum health at the start of the game 
        // and update the health UI to reflect the initial health value
        currentHealth = maxHealth;
        UpdateHealthUI();
        // Get the Animator component from the child object to play death animations when the player 
        // dies
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int amount)
    {
        // If the player is currently invincible from recent damage, return early to prevent applying
        // damage
        if (Time.time - lastDamageTime < invincibleTime)
            return;
        // If the player is already dead, return early to prevent applying damage or playing death
        // animations multiple times
        if (currentHealth <= 0 || isDead)
            return;
        // Reduce the current health by the damage amount and clamp it to a minimum of 0
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        // Update the health UI to reflect the new health value after taking damage
        UpdateHealthUI();
        lastDamageTime = Time.time;
        // For debugging purposes only, log the current health to the console after taking damage
        Debug.Log("Player Health: " + currentHealth);
        // If health has reached 0, call the Die method to handle death behavior
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        // If the player is already dead, return early to prevent healing a dead player
        if (isDead) return;
        // Increase the current health by the heal amount and clamp it to a maximum of maxHealth
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        // Update the health UI to reflect the new health value after healing
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        // If the health text UI element is assigned, update its text to show the current health as a
        // percentage of max health for easy readability by the player
        if (healthText != null)
            healthText.text = currentHealth + "%";
    }

    void Die()
    {
        // If the player is already marked as dead, return early to prevent running death behavior 
        // multiple times
        if (isDead) return;

        isDead = true;

        // For debugging purposes only, log a message to the console when the player 
        // dies to confirm that the death behavior is being triggered correctly
        Debug.Log("Player Died");

        // If an Animator component is assigned, set the "isDead" boolean parameter to true to trigger the 
        // death animation
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }

        // Disable player controls by setting the Rigidbody2D to kinematic and disabling the collider and
        // movement/shooting scripts to prevent any further input or movement after death
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable the player's collider to prevent further collisions with zombies or other objects after 
        // death
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Disable the PlayerMove script to prevent any further movement input after death
        PlayerMove move = GetComponent<PlayerMove>();
        if (move != null)
        {
            move.enabled = false;
        }

        // Disable the PlayerShooter script to prevent any further shooting input after death
        PlayerShooter shooter = GetComponent<PlayerShooter>();
        if (shooter != null)
        {
            shooter.enabled = false;
        }

        if (GameStats.Instance != null)
        {
            GameStats.Instance.SaveHighScores();
        }

        // Start the coroutine to go to Game Over after a delay,
        // allowing time for the death animation to play
        StartCoroutine(GoToGameOverAfterDelay());
}

IEnumerator GoToGameOverAfterDelay()
{
    yield return new WaitForSeconds(2f);

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;

    SceneManager.LoadScene("GameOver");
}
}