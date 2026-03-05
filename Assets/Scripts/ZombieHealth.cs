using UnityEngine;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;
    public int currentHealth;

    [Header("UI (assign in Inspector)")]
    public Image healthBarFill; 

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
        // Possibly death animation later, for now just destroy zombie
        Destroy(gameObject);
    }
}