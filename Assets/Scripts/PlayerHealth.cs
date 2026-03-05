using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Image healthBarFill;

    [Header("Invincibility")]
    public float invincibleTime = 1f;
    private float lastDamageTime;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }
    }

    public void TakeDamage(int amount)
    {
        // invincibility window
        if (Time.time - lastDamageTime < invincibleTime)
            return;

        if (currentHealth <= 0)
            return;

        currentHealth -= amount;

        // clamp so it never goes below 0
        currentHealth = Mathf.Max(currentHealth, 0);

        // update UI
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        lastDamageTime = Time.time;

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died");

        // disable movement
        PlayerMove move = GetComponent<PlayerMove>();
        if (move != null)
        {
            move.enabled = false;
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}
