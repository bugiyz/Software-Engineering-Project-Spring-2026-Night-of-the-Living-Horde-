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
    public LayerMask obstacleLayer; // set to Walls layer later

    [Header("Chase")]
    public float chaseSpeed = 2.5f;

    [Header("Roam")]
    public float roamSpeed = 1.4f;
    public float roamDirectionChangeMin = 1.0f;
    public float roamDirectionChangeMax = 2.5f;
    public float roamTurnSharpness = 8f; // higher = more responsive
    public float avoidDistance = 0.8f;   // how far ahead to check walls

    [Header("Memory")]
    public float memorySeconds = 1.5f;

    Rigidbody2D rb;
    Transform player;
    bool chasing;
    float lastTimePlayerSeen;

    Vector2 roamDir;
    float nextRoamChangeTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        PickNewRoamDirection();
    }

    void FixedUpdate()
    {
        // --- Detection / state update ---
        AcquireOrUpdateTarget();

        // --- Movement ---
        if (chasing && player != null)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.velocity = dir * chaseSpeed;
        }
        else
        {
            Roam();
        }
    }

    void AcquireOrUpdateTarget()
    {
        // If we don't have a player reference yet, try to acquire when close
        if (player == null)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            if (hit == null) return;

            if (requireLineOfSight && !HasLineOfSight(hit.transform))
                return;

            player = hit.transform;
            chasing = true;
            lastTimePlayerSeen = Time.time;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // Refresh "seen" time if LOS is allowed OR LOS is clear
        if (!requireLineOfSight || HasLineOfSight(player))
        {
            lastTimePlayerSeen = Time.time;
        }

        // If not chasing, start chasing once inside detection radius
        if (!chasing && dist <= detectionRadius)
        {
            if (!requireLineOfSight || HasLineOfSight(player))
            {
                chasing = true;
                lastTimePlayerSeen = Time.time;
            }
        }

        // Stop chasing when far + memory expired
        bool tooFar = dist > loseRadius;
        bool memoryExpired = (Time.time - lastTimePlayerSeen) > memorySeconds;

        if (tooFar && memoryExpired)
        {
            chasing = false;
            player = null;
            PickNewRoamDirection(); // immediately resume wandering
        }
    }

    void Roam()
    {
        // Change direction occasionally
        if (Time.time >= nextRoamChangeTime)
            PickNewRoamDirection();

        // Wall avoidance: if something is in front, steer away
        Vector2 adjusted = AvoidWalls(roamDir);

        // Smooth turning so it doesn't snap
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
        // If no obstacle layer is set or not using obstacles yet, just roam normally
        if (obstacleLayer.value == 0) return desiredDir;

        Vector2 origin = rb.position;

        // Raycast forward to see if we're about to hit a wall
        RaycastHit2D hit = Physics2D.Raycast(origin, desiredDir, avoidDistance, obstacleLayer);
        if (hit.collider == null) return desiredDir;

        // If blocked, try turning left or right (pick whichever is clearer)
        Vector2 left = new Vector2(-desiredDir.y, desiredDir.x);
        Vector2 right = new Vector2(desiredDir.y, -desiredDir.x);

        bool leftClear = Physics2D.Raycast(origin, left, avoidDistance, obstacleLayer).collider == null;
        bool rightClear = Physics2D.Raycast(origin, right, avoidDistance, obstacleLayer).collider == null;

        if (leftClear && !rightClear) return left;
        if (rightClear && !leftClear) return right;

        // If both blocked or both clear, just pick one randomly
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}