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

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateBar();

        // Play Hurt Sound
        audioSource.PlayOneShot(hurtClip);

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
    if (audioSource != null && deathClip != null)
        audioSource.PlayOneShot(deathClip);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Die");

        ZombieAI ai = GetComponent<ZombieAI>();
        if (ai != null)
            ai.enabled = false;

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

    // wait for death animation to finish
    float waitTime = 0.5f;

    // Keep sprite visible while death sound plays
    if (deathClip != null)
        waitTime = Mathf.Max(waitTime, deathClip.length);

    yield return new WaitForSeconds(waitTime);

    OnZombieDeath?.Invoke();
    Destroy(gameObject);
}
}