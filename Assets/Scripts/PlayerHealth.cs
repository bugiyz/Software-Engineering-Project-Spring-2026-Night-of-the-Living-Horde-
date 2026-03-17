using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    [Header("Invincibility")]
    public float invincibleTime = 1f;
    private float lastDamageTime;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastDamageTime < invincibleTime)
            return;

        if (currentHealth <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthUI();
        lastDamageTime = Time.time;

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = currentHealth + "%";
    }

    void Die()
    {
        Debug.Log("Player Died");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        PlayerMove move = GetComponent<PlayerMove>();
        if (move != null)
        {
            move.enabled = false;
        }

        PlayerShooter shooter = GetComponent<PlayerShooter>();
        if (shooter != null)
        {
            shooter.enabled = false;
        }

        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MainMenu");
    }
}