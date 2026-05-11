using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Pathfinding;

// This script controls the behavior of a zombie that uses pathfinding to navigate to the player.

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
public class ZombiePathfindingAI : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;
    public LayerMask playerLayer;
    public Transform attackPoint;
    public Animator animator;

    public Transform visualRoot;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip idleClip;
    public AudioClip attackClip;

    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    private Transform player;
    private float nextAttackTime;
    private Vector2 lastMoveDir = Vector2.down;
    private bool IsDead = false;

    [SerializeField] private Transform visual;
    [SerializeField] private float rotationOffset = 0f;

    void Awake()
    {
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        // If no Animator is assigned in the Inspector, grab one from a child
        // object such as the Square visual child.
       if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (visual == null && animator != null)
        {
            visual = animator.transform;
        }

        if (visualRoot == null && animator != null)
        {
            visualRoot = animator.transform;
        }
    }

    void Start()
    {
        // Give the zombie a reference to the player to locate him.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        // If a player object is found, set it as the target for the zombie.
        if (playerObj != null)
        {
            player = playerObj.transform;
            destinationSetter.target = player;
        }
        // If audio source is assigned and idle clip is assigned, play the idle sound.
        if (audioSource != null && idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void Update()
    {
        // If player is not found or zombie is dead, return early.
        if (player == null || IsDead) return;
        // set the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        // If the zombie is within attack range, stop moving and attack
        if (distanceToPlayer <= attackRange)
        {
            // Stop the zombie from moving
            aiPath.canMove = false;
            // If it's time to attack, perform the attack
            if (Time.time >= nextAttackTime)
            {
                Attack();
                // Set the next attack time
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // Allow the zombie to move
            aiPath.canMove = true;
        }
        // Update the zombie's animation
        UpdateAnimator();
    }

    void Attack()
{
    // perform the attack
    if (animator != null)
    {
        animator.SetTrigger("Attack");
    }

    // Play attack sound
    if (audioSource != null && attackClip != null)
    {
        audioSource.PlayOneShot(attackClip);
    }
    // Check for collisions with the player
    Vector2 center = attackPoint != null ? attackPoint.position : transform.position;
    // create a circle collider to determine if the player is in attack damage range
    Collider2D hit = Physics2D.OverlapCircle(center, attackRange, playerLayer);
    // If a player is hit, damage them
    if (hit != null)
    {
        PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
        // If the player has health, damage them
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
    // function to update the zombie's animation
    void UpdateAnimator()
{
    // if no animator is available, return early
    if (animator == null) return;
    // get the zombie's velocity
    Vector2 velocity = aiPath.desiredVelocity;
    // if the velocity > 0.1, update the last move direction
    if (velocity.magnitude > 0.1f)
    {
        // Update the last move direction
        lastMoveDir = velocity.normalized;
    }

    // Rotate the visual child toward movement direction
    if (visual != null && lastMoveDir.sqrMagnitude > 0.01f)
    {
        float angle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg;
        visual.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }
    // Update the animator parameters
    animator.SetFloat("MoveX", lastMoveDir.x);
    animator.SetFloat("MoveY", lastMoveDir.y);
    animator.SetFloat("Speed", velocity.sqrMagnitude);
}
// function to handle the zombie's death
public void Die()
{
    // If zombie is already dead, return early
    if (IsDead) return;
    // Stop the zombie's audio
    if (audioSource != null)
{
    audioSource.Stop();
}
    // Set the zombie as dead
    IsDead = true;
    // Disable the zombie's AI path
    if (aiPath != null)
    {
        aiPath.canMove = false;
        aiPath.canSearch = false;
        aiPath.isStopped = true;
        aiPath.maxSpeed = 0f;
        aiPath.enabled = false;
    }

    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    // Freeze the zombie's physics
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
    }
    // Set the zombie's death animation
    if (animator != null)
    {
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Die");
    }
    // Freeze the zombie's visual position
    StartCoroutine(FreezeVisualPosition());
    enabled = false;
}
// function to freeze the zombie's visual position
IEnumerator FreezeVisualPosition()
{
    if (visualRoot == null) yield break;

    Vector3 lockedPos = visualRoot.position;

    while (true)
    {
        visualRoot.position = lockedPos;
        yield return null;
    }
}
    // function to draw the attack range in the editor for debugging
    void OnDrawGizmosSelected()
    {
        Vector3 center = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}