using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

// this script manages the health of a zombie
public class ZombieHealth : MonoBehaviour
{
    public event Action OnZombieDeath;

    [Header("Health")]
    public int maxHealth = 50;
    public int currentHealth;

    [Header("UI (assign in Inspector)")]
    public Image healthBarFill;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hurtClip;
    public AudioClip deathClip;

    [Header("Drops")]
    [SerializeField] GameObject healthPickupPrefab;
    [SerializeField] float healthDropChance = 0.6f; // 60% chance

    [SerializeField] GameObject energyDrinkPrefab;
    [SerializeField] float energyDropChance = 0.25f; // 25% chance

    private bool isDead = false;

    void Start()
    {
        // Initialize the zombie's health
        currentHealth = maxHealth;
        // Initialize the health bar
        UpdateBar();
    }
    // function to take damage
    public void TakeDamage(int amount)
    {
        // If the zombie is already dead or has no health, return early
        if (isDead || currentHealth <= 0) return;
        // If no damage is to be taken, return early
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        // Update the health bar
        UpdateBar();
        // If the zombie is still alive, play the hurt sound
        if (currentHealth > 0)
        {
            if (audioSource != null && hurtClip != null)
                audioSource.PlayOneShot(hurtClip);
        }
        // If the zombie has no health left, it dies
        if (currentHealth == 0)
            Die();
    }
    // function to update the health bar
    void UpdateBar()
    {
        // if health bar is assigned, update its fill amount
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        // If the zombie is already dead, return early
        if (isDead) return;
        isDead = true;
        // If audio source is assigned, play the death sound
        if (audioSource != null && deathClip != null)
            audioSource.PlayOneShot(deathClip);
        // If the zombie has a death animation, trigger it
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            if (HasTriggerParameter(anim, "Die"))
                anim.SetTrigger("Die");

            if (HasBoolParameter(anim, "IsDead"))
                anim.SetBool("IsDead", true);
        }

        ZombieAI oldAI = GetComponent<ZombieAI>();
        if (oldAI != null)
            oldAI.enabled = false;

        ZombiePathfindingAI pathAI = GetComponent<ZombiePathfindingAI>();
        if (pathAI != null)
        {
            pathAI.Die();
            pathAI.enabled = false;
        }

        StartCoroutine(DeathRoutine());
    }
    // function to handle the zombie's death routine
    IEnumerator DeathRoutine()
    {
        // get the zombie's collider and rigidbody
        Collider2D col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // Disable the zombie's collider and rigidbody
        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        // Wait for a short time before destroying the zombie
        float waitTime = 0.5f;
        // If the death clip is assigned, use its length as the wait time
        if (deathClip != null)
            waitTime = Mathf.Max(waitTime, deathClip.length);
        // If the zombie has a death animation, wait for it to finish
        yield return new WaitForSeconds(waitTime);
        // Drop a item randomly
        float roll = UnityEngine.Random.value;
        // Create a new instance of the drop prefab at the zombie's position
        GameObject dropPrefab = null;
        // If the roll is less than the health drop chance, drop a health pickup
        if (roll < healthDropChance)
        {
            dropPrefab = healthPickupPrefab;
        }
        // If the roll is less than the health drop chance plus the energy drop chance, drop an energy drink
        else if (roll < healthDropChance + energyDropChance)
        {
            dropPrefab = energyDrinkPrefab;
        }

        // If a drop prefab is assigned, instantiate it at the zombie's position
        if (dropPrefab != null)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0f
            );
    // Add a small random offset to the zombie's position   
    Instantiate(dropPrefab, transform.position + offset, Quaternion.identity);
}
        // Notify any listeners that the zombie has died
        OnZombieDeath?.Invoke();
        // Update the game stats
        if (GameStats.Instance != null)
        {
            GameStats.Instance.AddZombieKill();
        }
        // Destroy the zombie game object
        Destroy(gameObject);
    }
    // function to check if an animator has a specific trigger parameter
    bool HasTriggerParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }
    // function to check if an animator has a specific bool parameter
    bool HasBoolParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                return true;
        }
        return false;
    }
}