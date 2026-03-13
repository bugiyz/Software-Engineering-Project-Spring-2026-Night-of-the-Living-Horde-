using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float runSpeed = 8f;

    // UI
    public Image StaminaBar;
    public CanvasGroup staminaCanvas;

    // Stamina
    public float maxStamina = 100f;
    public float stamina;

    public float runStaminaDrain = 20f;
    public float dashCost = 30f;
    public float staminaRegen = 15f;

    // Dash
    public float dashSpeed = 20f;
    public float dashTime = 0.15f;

    // Exhaustion
    public float exhaustedRecoveryDelay = 2f;
    private float exhaustedTimer = 0f;
    private bool exhausted = false;

    // UI behavior
    public float hideDelay = 1f;
    public float fadeSpeed = 3f;
    private float hideTimer = 0f;

    Rigidbody2D rb;

    bool isDashing = false;
    Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stamina = maxStamina;
        
        if (staminaCanvas != null)
            staminaCanvas.alpha = 0f;
    }

    void Update()
    {
        // DASH
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && stamina >= dashCost && !exhausted)
        {
            stamina -= dashCost;

            if (stamina <= 0)
            {
                stamina = 0;
                exhausted = true;
                exhaustedTimer = exhaustedRecoveryDelay;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            dashDirection = new Vector2(h, v).normalized;

            if (dashDirection == Vector2.zero)
                dashDirection = Vector2.up;

            StartCoroutine(Dash());
        }

        HandleStaminaRegen();
        UpdateStaminaUI();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v).normalized;
        float currentSpeed = speed;

        // RUN
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !exhausted)
        {
            currentSpeed = runSpeed;

            stamina -= runStaminaDrain * Time.fixedDeltaTime;

            if (stamina <= 0)
            {
                stamina = 0;
                exhausted = true;
                exhaustedTimer = exhaustedRecoveryDelay;
            }
        }

        rb.velocity = move * currentSpeed;
    }

    void HandleStaminaRegen()
    {
        if (exhausted)
        {
            exhaustedTimer -= Time.deltaTime;

            if (exhaustedTimer <= 0)
                exhausted = false;

            return;
        }

        if (!Input.GetKey(KeyCode.LeftShift) && !isDashing && stamina < maxStamina)
        {
            stamina += staminaRegen * Time.deltaTime;

            if (stamina > maxStamina)
                stamina = maxStamina;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }

    void UpdateStaminaUI()
    {
        if (StaminaBar != null)
            StaminaBar.fillAmount = stamina / maxStamina;

        if (staminaCanvas == null) return;

        if (stamina < maxStamina)
        {
            // Appear instantly when stamina is used
            staminaCanvas.alpha = 1f;
            hideTimer = 0f;
        }
        else
        {
            hideTimer += Time.deltaTime;

            if (hideTimer >= hideDelay)
            {
                // Fade out smoothly when full
                staminaCanvas.alpha = Mathf.Lerp(staminaCanvas.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
    }
}