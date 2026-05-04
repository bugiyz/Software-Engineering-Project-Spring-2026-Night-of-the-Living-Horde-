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

    [Header("Drops")]
    [SerializeField] GameObject healthPickupPrefab;
    [SerializeField] float healthDropChance = 0.6f; // 60% chance

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

        // ? 60% chance to drop health
        if (UnityEngine.Random.value < healthDropChance && healthPickupPrefab != null)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0f
            );

            Instantiate(healthPickupPrefab, transform.position + offset, Quaternion.identity);
        }

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