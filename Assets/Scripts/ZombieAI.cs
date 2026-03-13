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
    public float avoidDistance = 1.5f;

    [Header("Memory")]
    public float memorySeconds = 1.5f;

    [Header("Animator")]
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public float animDampTime = 0.08f;

    [Header("Attack")]
    public float attackRange = 1.3f;
    public float attackCooldown = 1.0f;
    public float attackWindup = 0.10f;
    public string isAttackingParam = "IsAttacking";

    [Header("Attack Hit")]
    public Transform attackPoint;
    public float hitRadius = 0.28f;
    public LayerMask playerHitLayer;
    public int attackDamage = 10;

    [Header("Run Burst")]
    public float runBurstMultiplier = 1.6f;
    public float runBurstSeconds = 0.8f;
    float runBurstEndTime;

    [Header("Run Visual")]
    public float normalAnimSpeed = 1.0f;
    public float runAnimSpeed = 1.35f;

    float nextAttackTime;
    Vector2 lastMoveDir = Vector2.down;

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

        if (player != null)
        {
            float dist = Vector2.Distance(rb.position, (Vector2)player.position);

            if (dist <= attackRange)
            {
                TryAttack();
                rb.velocity = Vector2.zero;
                UpdateAnimatorFacing(lastMoveDir);
                return;
            }
            else
            {
                if (anim) anim.SetBool(isAttackingParam, false);
            }
        }

        if (chasing && player != null)
        {
            Vector2 desiredDir = ((Vector2)player.position - rb.position).normalized;
            Vector2 adjustedDir = AvoidWalls(desiredDir);

            float speed = chaseSpeed;
            if (Time.time < runBurstEndTime)
                speed *= runBurstMultiplier;

            Vector2 targetVelocity = adjustedDir * speed;
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, roamTurnSharpness * Time.fixedDeltaTime);

            if (anim)
                anim.speed = (Time.time < runBurstEndTime) ? runAnimSpeed : normalAnimSpeed;
        }
        else
        {
            Roam();
        }

        if (rb.velocity.sqrMagnitude > 0.0001f)
            lastMoveDir = rb.velocity.normalized;

        UpdateAnimator(rb.velocity);
    }

    void AcquireOrUpdateTarget()
    {
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
        Vector2 dir = desiredDir.normalized;
        float lookAhead = avoidDistance;

        RaycastHit2D forwardHit = Physics2D.Raycast(origin, dir, lookAhead, obstacleLayer);

        if (forwardHit.collider == null)
            return dir;

        Vector2 left = new Vector2(-dir.y, dir.x).normalized;
        Vector2 right = new Vector2(dir.y, -dir.x).normalized;

        Vector2 leftDir = (dir + left).normalized;
        Vector2 rightDir = (dir + right).normalized;

        RaycastHit2D leftHit = Physics2D.Raycast(origin, leftDir, lookAhead, obstacleLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(origin, rightDir, lookAhead, obstacleLayer);

        bool leftBlocked = leftHit.collider != null;
        bool rightBlocked = rightHit.collider != null;

        if (!leftBlocked && rightBlocked)
            return leftDir;

        if (!rightBlocked && leftBlocked)
            return rightDir;

        if (!leftBlocked && !rightBlocked)
            return (Random.value < 0.5f) ? leftDir : rightDir;

        return left;
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

        if (speed < 0.01f)
            return;

        Vector2 dir = velocity / speed;

        anim.SetFloat(moveXParam, dir.x, animDampTime, Time.fixedDeltaTime);
        anim.SetFloat(moveYParam, dir.y, animDampTime, Time.fixedDeltaTime);
    }

    void TryAttack()
    {
        if (!anim) return;

        anim.SetBool(isAttackingParam, true);

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        DealDamage();
    }

    void DealDamage()
    {
        if (!attackPoint) return;

        Vector2 offset = lastMoveDir.normalized * 0.35f;
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