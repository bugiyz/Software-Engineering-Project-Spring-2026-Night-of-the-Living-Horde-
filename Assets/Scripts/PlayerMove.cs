using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float runSpeed = 8f;

    public float dashSpeed = 20f;
    public float dashTime = 0.15f;

    // Stamina system
    public float maxStamina = 100f;
    public float stamina;

    public float runStaminaDrain = 20f; // per second
    public float dashCost = 30f;
    public float staminaRegen = 15f; // per second

    // Exhaustion system
    public float exhaustedRecoveryDelay = 2f;
    private float exhaustedTimer = 0f;
    private bool exhausted = false;

    Rigidbody2D rb;

    bool isDashing = false;
    Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stamina = maxStamina;
    }

    void Update()
    {
        // Dash
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && stamina > 0 && !exhausted)
        {
            // Subtract stamina and clamp
            stamina -= dashCost;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

            // If stamina hits 0, trigger exhaustion
            if (stamina == 0)
            {
                exhausted = true;
                exhaustedTimer = exhaustedRecoveryDelay;
            }

            // Get dash direction
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            dashDirection = new Vector2(h, v).normalized;

            if (dashDirection == Vector2.zero)
                dashDirection = Vector2.up;

            StartCoroutine(Dash());
        }

        HandleStaminaRegen();
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

        // Running
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !exhausted)
        {
            currentSpeed = runSpeed;
            stamina -= runStaminaDrain * Time.fixedDeltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

            // Trigger exhaustion if stamina hits 0
            if (stamina == 0)
            {
                exhausted = true;
                exhaustedTimer = exhaustedRecoveryDelay;
            }
        }

        rb.velocity = move * currentSpeed;
    }

    void HandleStaminaRegen()
    {
        // Exhaustion: wait before regenerating
        if (exhausted)
        {
            exhaustedTimer -= Time.deltaTime;
            if (exhaustedTimer <= 0)
                exhausted = false;

            return;
        }

        // Normal regen when not using stamina
        if (!Input.GetKey(KeyCode.LeftShift) && !isDashing && stamina < maxStamina)
        {
            stamina += staminaRegen * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }
}