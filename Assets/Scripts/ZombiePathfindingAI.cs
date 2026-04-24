using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Pathfinding;

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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            destinationSetter.target = player;
        }

        if (audioSource != null && idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (player == null || IsDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            aiPath.canMove = false;

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            aiPath.canMove = true;
        }

        UpdateAnimator();
    }

    void Attack()
{
    if (animator != null)
    {
        animator.SetTrigger("Attack");
    }

    // 🔊 Play attack sound
    if (audioSource != null && attackClip != null)
    {
        audioSource.PlayOneShot(attackClip);
    }

    Vector2 center = attackPoint != null ? attackPoint.position : transform.position;
    Collider2D hit = Physics2D.OverlapCircle(center, attackRange, playerLayer);

    if (hit != null)
    {
        PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}

    void UpdateAnimator()
{
    if (animator == null) return;

    Vector2 velocity = aiPath.desiredVelocity;

    if (velocity.magnitude > 0.1f)
    {
        lastMoveDir = velocity.normalized;
    }

    // Rotate the visual child toward movement direction
    if (visual != null && lastMoveDir.sqrMagnitude > 0.01f)
    {
        float angle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg;
        visual.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    animator.SetFloat("MoveX", lastMoveDir.x);
    animator.SetFloat("MoveY", lastMoveDir.y);
    animator.SetFloat("Speed", velocity.sqrMagnitude);
}

public void Die()
{
    if (IsDead) return;

    if (audioSource != null)
{
    audioSource.Stop();
}

    IsDead = true;

    if (aiPath != null)
    {
        aiPath.canMove = false;
        aiPath.canSearch = false;
        aiPath.isStopped = true;
        aiPath.maxSpeed = 0f;
        aiPath.enabled = false;
    }

    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
    }

    if (animator != null)
    {
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Die");
    }

    StartCoroutine(FreezeVisualPosition());
    enabled = false;
}

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

    void OnDrawGizmosSelected()
    {
        Vector3 center = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}