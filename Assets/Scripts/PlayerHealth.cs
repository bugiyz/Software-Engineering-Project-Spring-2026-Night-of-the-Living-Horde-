using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // Stop physics movement immediately
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // Disable movement
    PlayerMove move = GetComponent<PlayerMove>();
    if (move != null)
    {
        move.enabled = false;
    }

    // Disable shooting
    PlayerShooter shooter = GetComponent<PlayerShooter>();
    if (shooter != null)
    {
        shooter.enabled = false;
    }

    // Return to main menu after 3 seconds
        StartCoroutine(ReturnToMainMenu());
   
    System.Collections.IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MainMenu");
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
