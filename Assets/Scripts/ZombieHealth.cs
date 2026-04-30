using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

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

    

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        if (isDead || currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateBar();

        if (currentHealth > 0)
        {
            if (audioSource != null && hurtClip != null)
                audioSource.PlayOneShot(hurtClip);
        }

        if (currentHealth == 0)
            Die();
    }

    void UpdateBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (audioSource != null && deathClip != null)
            audioSource.PlayOneShot(deathClip);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            // Old zombies can still use a trigger named "Die"
            if (HasTriggerParameter(anim, "Die"))
                anim.SetTrigger("Die");

            // New pathfinding zombie can use a bool named "IsDead"
            if (HasBoolParameter(anim, "IsDead"))
                anim.SetBool("IsDead", true);
        }

        // Stop old zombie AI if present
        ZombieAI oldAI = GetComponent<ZombieAI>();
        if (oldAI != null)
            oldAI.enabled = false;

        // Stop new pathfinding AI if present
        ZombiePathfindingAI pathAI = GetComponent<ZombiePathfindingAI>();
        if (pathAI != null)
        {
            pathAI.Die(); // Call Die() to handle stopping movement and other logic
            pathAI.enabled = false;
        }

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        Collider2D col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        float waitTime = 0.5f;

        if (deathClip != null)
            waitTime = Mathf.Max(waitTime, deathClip.length);

        yield return new WaitForSeconds(waitTime);

        OnZombieDeath?.Invoke();
        Destroy(gameObject);
    }

    bool HasTriggerParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }

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