using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script is responsible for handling player movement, including walking, running, dashing,
// stamina management, and updating the stamina UI. The player can walk at a normal speed, 
// run by holding Shift, and dash by pressing Space.

public class PlayerMove : MonoBehaviour
{
    // Movement speeds for walking and running, set in the inspector for easy tweaking
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

    // Components
    private Rigidbody2D rb;
    private Animator animator;

    // Visual child that holds the sprite
    public Transform visual;

    // Movement / dash
    private bool isDashing = false;
    private Vector2 dashDirection;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

   void Awake()
{
    // Get rigidBody component for movement and intialize stamina to max at start of game
    rb = GetComponent<Rigidbody2D>();
    stamina = maxStamina;
    /*
    if (staminaCanvas != null)
        staminaCanvas.alpha = 0f;
*/
    // Check if a visual transform is assigned in current game object
    if (visual == null)
    {
        // if not, try to find animator in the child objects
        Animator childAnimator = GetComponentInChildren<Animator>();
        if (childAnimator != null)
        {
            animator = childAnimator;
            visual = childAnimator.transform;
        }
    }
    // If a visual transform is assigned, get the Animator component from it to control animations
    else
    {
        animator = visual.GetComponent<Animator>();
    }

    // Hide cursor for gameplay Will be using custom crosshair sprite during gameplay
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Confined;
}

    void Update()
    {
        // If the player is currently exhausted, decrement the exhausted timer and 
        // return early to prevent
        // Read movement input using the Input.GetAxisRaw method for eaier input handling than 
        // defining individual key inputs. Also normalize the input vector to prevent faster
        // diagonal movement for smoother movement in all directions
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // Store last non-zero move direction
        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
        }

        // Update animator
        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != Vector2.zero);
        }
        // Rotate the visual child to face the mouse position for aiming
        RotateToMouse();

        // Check if key for dashing is pressed if so start the dash if player has enough stamina 
        // and is not currently exhausted or dashing
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && stamina >= dashCost && !exhausted)
        {
            stamina -= dashCost;

            if (stamina <= 0)
            {
                stamina = 0;
                exhausted = true;
                exhaustedTimer = exhaustedRecoveryDelay;
            }

            dashDirection = moveInput;
            // If no movement input, dash in the last move direction for better responsiveness
            if (dashDirection == Vector2.zero)
                dashDirection = lastMoveDirection;
            // Start the dash routine to handle the dash movement and timing
            StartCoroutine(Dash());
        }
        // Handle stamina regeneration and update the stamina UI every frame
        HandleStaminaRegen();
        UpdateStaminaUI();
    }
    // Use FixedUpdate for consistent physics-based movement regardless of frame rate
    void FixedUpdate()
    {
        // If currently dashing, set velocity to dash direction and 
        // speed and return early to skip normal movement
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
            return;
        }
        // Determine the current movement speed based on whether the player is running or walking
        float currentSpeed = speed;

        // If player is holding Left shift key start running and drain stamina accordingly, 
        // also check if player has enough stamina to run and is not currently exhausted and 
        // is actually moving
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !exhausted && moveInput != Vector2.zero)
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
        // Set the player's velocity based on the movement input and current speed for smooth movement
        rb.velocity = moveInput * currentSpeed;
    }

    void HandleStaminaRegen()
    {
        // If the player is currently exhausted, decrement the exhausted timer and return early 
        // to prevent stamina regeneration until the exhaustion period has ended
        if (exhausted)
        {
            exhaustedTimer -= Time.deltaTime;

            if (exhaustedTimer <= 0)
                exhausted = false;

            return;
        }
        // If player is not holding shift and not dashing and stamina is below max, regenerate stamina
        if (!Input.GetKey(KeyCode.LeftShift) && !isDashing && stamina < maxStamina)
        {
            stamina += staminaRegen * Time.deltaTime;

            if (stamina > maxStamina)
                stamina = maxStamina;
        }
    }

    IEnumerator Dash()
    {
        // Set the isDashing flag to true to indicate that the player is currently dashing and 
        // should be moved in the dash direction at dash speed in FixedUpdate, then wait for 
        // the dash time
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }

    void UpdateStaminaUI()
    {
        // If the stamina bar UI image is assigned, update its fill amount based on the current stamina 
        // percentage
        if (StaminaBar != null)
            StaminaBar.fillAmount = stamina / maxStamina;
       
    }
    void RotateToMouse()
    {
        // If no visual transform is assigned, return early since we have no reference for aiming 
        // direction
        if (visual == null) return;
        // Get the mouse position in world coordinates and calculate the direction from the visual
        // transform to the mouse position to determine the angle to rotate the visual for aiming
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - visual.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Because sprite faces DOWN by default
        visual.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }
}