using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ZombieAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 6f;
    public float loseRadius = 8f;
    public bool requireLineOfSight = false;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Chase")]
    public float chaseSpeed = 2.5f;

    [Header("Roam")]
    public float roamSpeed = 1.4f;
    public float roamDirectionChangeMin = 1.0f;
    public float roamDirectionChangeMax = 2.5f;
    public float roamTurnSharpness = 8f;
    public float avoidDistance = 0.8f;

    [Header("Memory")]
    public float memorySeconds = 1.5f;

    [Header("Animator")]
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public float animDampTime = 0.08f;

    [Header("Attack")]
    public float attackRange = 1.3f;
    public float attackCooldown = 1.0f;     // time between hits
    public float attackWindup = 0.10f;      // small delay before hit (optional)
    public string isAttackingParam = "IsAttacking";
    [Header("Attack Hit")]
    public Transform attackPoint;
    public float hitRadius = 0.28f;
    public LayerMask playerHitLayer;   // set to Player layer
    public int attackDamage = 10;

    [Header("Run Burst")]
    public float runBurstMultiplier = 1.6f;   // 1.3–2.0 feels good
    public float runBurstSeconds = 0.8f;      // 0.4–1.2
    float runBurstEndTime;

    [Header("Run Visual")]
    public float normalAnimSpeed = 1.0f;
    public float runAnimSpeed = 1.35f;

    float nextAttackTime;
    Vector2 lastMoveDir = Vector2.down;     // remembers last facing direction

    Rigidbody2D rb;
    Transform player;
    bool chasing;
    float lastTimePlayerSeen;

    Vector2 roamDir;
    float nextRoamChangeTime;

    Animator anim;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        PickNewRoamDirection();
    }

    void FixedUpdate()
{
    AcquireOrUpdateTarget();

    // If we have a player, check if we should attack
    if (player != null)
    {
        float dist = Vector2.Distance(rb.position, (Vector2)player.position);

        if (dist <= attackRange)
        {
            TryAttack();
            // During attack, we stop movement
            rb.velocity = Vector2.zero;

            // Feed animator using last facing direction works with attack blend to stay in correct direction
            UpdateAnimatorFacing(lastMoveDir);
            return;
        }
        else
        {
            // Not in range -> ensure we are not in attack state for animation
            if (anim) anim.SetBool(isAttackingParam, false);
        }
    }

    // Normal movement
    if (chasing && player != null)
{
    Vector2 dir = ((Vector2)player.position - rb.position).normalized;

    float speed = chaseSpeed;
    if (Time.time < runBurstEndTime)
        speed *= runBurstMultiplier;

    rb.velocity = dir * speed;

    // adjust animation speed when running
    if (anim)
        anim.speed = (Time.time < runBurstEndTime) ? runAnimSpeed : normalAnimSpeed;
    }
    else
    {
    Roam();
    }

    // Save last direction whenever we are moving (for attack facing)
    if (rb.velocity.sqrMagnitude > 0.0001f)
        lastMoveDir = rb.velocity.normalized;

    UpdateAnimator(rb.velocity);
}

    void AcquireOrUpdateTarget()
    {
        runBurstEndTime = Time.time + runBurstSeconds;
        if (player == null)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            if (hit == null) return;

            if (requireLineOfSight && !HasLineOfSight(hit.transform))
                return;

            player = hit.transform;
            chasing = true;
            lastTimePlayerSeen = Time.time;
            runBurstEndTime = Time.time + runBurstSeconds;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (!requireLineOfSight || HasLineOfSight(player))
            lastTimePlayerSeen = Time.time;

        if (!chasing && dist <= detectionRadius)
        {
            if (!requireLineOfSight || HasLineOfSight(player))
            {
                chasing = true;
                lastTimePlayerSeen = Time.time;
                runBurstEndTime = Time.time + runBurstSeconds;
            }
        }

        bool tooFar = dist > loseRadius;
        bool memoryExpired = (Time.time - lastTimePlayerSeen) > memorySeconds;

        if (tooFar && memoryExpired)
        {
            chasing = false;
            player = null;
            PickNewRoamDirection();
        }
    }

    void Roam()
    {
        if (anim)
            anim.speed = normalAnimSpeed;

        if (Time.time >= nextRoamChangeTime)
            PickNewRoamDirection();

        Vector2 adjusted = AvoidWalls(roamDir);
        roamDir = Vector2.Lerp(roamDir, adjusted, roamTurnSharpness * Time.fixedDeltaTime).normalized;

        rb.velocity = roamDir * roamSpeed;
    }

    void PickNewRoamDirection()
    {
        roamDir = Random.insideUnitCircle.normalized;
        float t = Random.Range(roamDirectionChangeMin, roamDirectionChangeMax);
        nextRoamChangeTime = Time.time + t;
    }

    Vector2 AvoidWalls(Vector2 desiredDir)
    {
        if (obstacleLayer.value == 0) return desiredDir;

        Vector2 origin = rb.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, desiredDir, avoidDistance, obstacleLayer);
        if (hit.collider == null) return desiredDir;

        Vector2 left = new Vector2(-desiredDir.y, desiredDir.x);
        Vector2 right = new Vector2(desiredDir.y, -desiredDir.x);

        bool leftClear = Physics2D.Raycast(origin, left, avoidDistance, obstacleLayer).collider == null;
        bool rightClear = Physics2D.Raycast(origin, right, avoidDistance, obstacleLayer).collider == null;

        if (leftClear && !rightClear) return left;
        if (rightClear && !leftClear) return right;

        return (Random.value < 0.5f) ? left : right;
    }

    bool HasLineOfSight(Transform target)
    {
        if (obstacleLayer.value == 0) return true;

        Vector2 origin = rb.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float dist = Vector2.Distance(origin, target.position);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, obstacleLayer);
        return hit.collider == null;
    }

    void UpdateAnimator(Vector2 velocity)
    {
        if (!anim) return;

        float speed = velocity.magnitude;

        // Don't spam direction changes if basically stopped
        if (speed < 0.01f)
            return;

        Vector2 dir = velocity / speed;

        // Drive the blend tree
        anim.SetFloat(moveXParam, dir.x, animDampTime, Time.fixedDeltaTime);
        anim.SetFloat(moveYParam, dir.y, animDampTime, Time.fixedDeltaTime);

    }

    void TryAttack()
    {
    if (!anim) return;

    // If we're in cooldown, stay in attack anim but don't "hit" again yet
    anim.SetBool(isAttackingParam, true);

    if (Time.time < nextAttackTime)
        return;

    nextAttackTime = Time.time + attackCooldown;

    DealDamage();
    }

void DealDamage()
{
    if (!attackPoint) return;

    Vector2 offset = lastMoveDir.normalized * 0.35f; // adjust distance
    attackPoint.localPosition = offset;

    Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, hitRadius, playerHitLayer);
    if (hit == null) return;

    var health = hit.GetComponent<PlayerHealth>();
    if (health != null)
        health.TakeDamage(attackDamage);
}

    void UpdateAnimatorFacing(Vector2 dir)
{
    if (!anim) return;

    // If dir is tiny, do nothing
    if (dir.sqrMagnitude < 0.0001f) return;

    anim.SetFloat(moveXParam, dir.x, animDampTime, Time.fixedDeltaTime);
    anim.SetFloat(moveYParam, dir.y, animDampTime, Time.fixedDeltaTime);
}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        if (attackPoint)
        {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, hitRadius);
        }
    }
}