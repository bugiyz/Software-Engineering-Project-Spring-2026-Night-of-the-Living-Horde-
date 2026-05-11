using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for controlling the zombie's AI behavior, 
// including chasing the player, roaming when no player is detected, 
// attacking when in range, and avoiding obstacles. It also interfaces 
// with the animator to update animation parameters based on movement and attack states.

[RequireComponent(typeof(Rigidbody2D))]
public class ZombieAI : MonoBehaviour
{
    //Setting variables in the inspector for easy tweaking of the zombie's behavior. 
    // Organized with headers for clarity.
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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip idleClip;
    public AudioClip attackClip;

    [Header("Visual Rotation")]
    [SerializeField] private Transform visual;
    [SerializeField] private float rotationOffset = 0f;
    
    //attack cooldown timer and last move direction for attack point positioning
    float nextAttackTime;
    Vector2 lastMoveDir = Vector2.down;
    //Creating variables for the rigidbody, player transform, chasing state, and timers for player 
    // sighting and roaming behavior
    Rigidbody2D rb;
    Transform player;
    bool chasing;
    float lastTimePlayerSeen;
    // Variables for roaming behavior, including current roam direction and timer for when to change 
    // direction
    Vector2 roamDir;
    float nextRoamChangeTime;
    // Creating variables for the animator and sprite renderer to control animations and facing direction
    Animator anim;
    SpriteRenderer sr;

    void Awake()
    {
        // Component references to avoid repeated GetComponent calls in the update loop, 
        // improving performance
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        // Intialize the zombies roam direction and timers, and start playing zombieidle sound
        PickNewRoamDirection();

        audioSource = GetComponent<AudioSource>();
        
        if (idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void FixedUpdate()
    {
        // First, check for the player and update the target if necessary. This handles acquiring 
        // a new target or losing the target if they go out of range or line of sight.
        AcquireOrUpdateTarget();
        // If player target exists, check distance to see if we should attack.
        if (player != null)
        {
            float dist = Vector2.Distance(rb.position, (Vector2)player.position);
            // If within attack range, stop moving and try to attack. Also update the animator to face the player.
            
            Debug.Log("Distance to player: " + dist);
            if (dist <= attackRange)
            {

                Debug.Log("In Attack Range");

                rb.velocity = Vector2.zero;

                Vector2 dirToPlayer = ((Vector2)player.position - rb.position).normalized;
                lastMoveDir = dirToPlayer;

                UpdateAnimatorFacing(dirToPlayer);
                TryAttack();

                return;
            }
        }
        // If we're chasing a target, calculate the desired direction towards the player, 
        // adjust it to avoid walls, and set the velocity.
        if (chasing && player != null)
        {
            Vector2 desiredDir = ((Vector2)player.position - rb.position).normalized;
            Vector2 adjustedDir = AvoidWalls(desiredDir);

            float speed = chaseSpeed;
            // If we're in a run burst, apply the speed multiplier and set the animation 
            // speed accordingly.
            if (Time.time < runBurstEndTime)
                speed *= runBurstMultiplier;

            Vector2 targetVelocity = adjustedDir * speed;
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, roamTurnSharpness * Time.fixedDeltaTime);
            // Set the animation speed based on whether we're in a run burst or not.
            if (anim)
                anim.speed = (Time.time < runBurstEndTime) ? runAnimSpeed : normalAnimSpeed;
        }
        // If we're not chasing, execute the roaming behavior which involves picking a direction,
        // avoiding walls, and moving in that direction.
        else
        {
            Roam();
        }
        // if we have a significant velocity, update the last move direction for use in 
        // attack point positioning and animation facing.
        if (rb.velocity.sqrMagnitude > 0.0001f)
        {
            lastMoveDir = rb.velocity.normalized;
        }
        UpdateAnimator(rb.velocity);
    }

    void AcquireOrUpdateTarget()
    {
        // If no player target
        if (player == null)
        {
            // Create a circle overlap to detect if the player is within the detection radius
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            // If we do not hit anything, return early
            if (hit == null) return;
            // Check line of sight if we do have a hit
            // but not line of sight, return early
            if (requireLineOfSight && !HasLineOfSight(hit.transform))
                return;
            //  If player detected and line of sight acquired, set the player target, start chasing
            // and initialize timers for player sighting and run burst.
            player = hit.transform;
            chasing = true;
            lastTimePlayerSeen = Time.time;
            runBurstEndTime = Time.time + runBurstSeconds;
            return;
        }
        // If we already have a player target, calculate the distance to the player
        float dist = Vector2.Distance(transform.position, player.position);
        // If we have line of sight to the player, update the last time we saw 
        // them to keep chasing even if they temporarily go out of sight.
        if (!requireLineOfSight || HasLineOfSight(player))
            lastTimePlayerSeen = Time.time;
        // Check if player is within detection radius
        if (!chasing && dist <= detectionRadius)
        {
            // Check if line of sight is aquired, if so start chasing and intialize
            // timers for player sighting and run burst.
            if (!requireLineOfSight || HasLineOfSight(player))
            {
                chasing = true;
                lastTimePlayerSeen = Time.time;
                runBurstEndTime = Time.time + runBurstSeconds;
            }
        }
        // Set bool values to see if player is too far or memory has exipred
        bool tooFar = dist > loseRadius;
        bool memoryExpired = (Time.time - lastTimePlayerSeen) > memorySeconds;
        // Check if player is too far or memory has expired, if so stop chasing, clear player target,
        // and pick a new roam direction to start wandering.
        if (tooFar && memoryExpired)
        {
            chasing = false;
            player = null;
            PickNewRoamDirection();
        }
    }

    void Roam()
    {
        // Set animation speed to normal when roaming
        if (anim)
            anim.speed = normalAnimSpeed;
        // Check if zombie has roamed in current direction long enough
        // If so, pick new direction to roam in
        if (Time.time >= nextRoamChangeTime)
            PickNewRoamDirection();
        // Adjust the roam direction to avoid walls and set the velocity accordingly
        Vector2 adjusted = AvoidWalls(roamDir);
        roamDir = Vector2.Lerp(roamDir, adjusted, roamTurnSharpness * Time.fixedDeltaTime).normalized;

        rb.velocity = roamDir * roamSpeed;
    }

    void PickNewRoamDirection()
    {
        // Pick a random direction to roam in and set a timer for when to change direction again
        roamDir = Random.insideUnitCircle.normalized;
        float t = Random.Range(roamDirectionChangeMin, roamDirectionChangeMax);
        nextRoamChangeTime = Time.time + t;
    }

    Vector2 AvoidWalls(Vector2 desiredDir)
    {
        // If no obstacle layer is set, just return the desired direction without doing any checks
        if (obstacleLayer.value == 0) return desiredDir;
        // cast a ray forwward to see if we hit a wall
        Vector2 origin = rb.position;
        Vector2 dir = desiredDir.normalized;
        float lookAhead = avoidDistance;

        RaycastHit2D forwardHit = Physics2D.Raycast(origin, dir, lookAhead, obstacleLayer);
        // If we don't hit anything, return the original desired direction
        if (forwardHit.collider == null)
            return dir;
        // If raycast hits an obstacle, we need to steer around it.
        // Create two more vectors to the left and right of current direction to check for
        // open paths
        Vector2 left = new Vector2(-dir.y, dir.x).normalized;
        Vector2 right = new Vector2(dir.y, -dir.x).normalized;
        // Combine the forward direction with the left and right directions
        Vector2 leftDir = (dir + left).normalized;
        Vector2 rightDir = (dir + right).normalized;
        // Cast rays in the left and right directions and check if they are clear of obstacles
        RaycastHit2D leftHit = Physics2D.Raycast(origin, leftDir, lookAhead, obstacleLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(origin, rightDir, lookAhead, obstacleLayer);
        // Set bool values to see if left or right paths are clear of obstacles
        bool leftBlocked = leftHit.collider != null;
        bool rightBlocked = rightHit.collider != null;
        // If Left clear and right blocked, return left direction
        if (!leftBlocked && rightBlocked)
            return leftDir;
        // If right clear and left blocked, return right direction
        if (!rightBlocked && leftBlocked)
            return rightDir;
        // if both paths are clearm choose randomly between left and right direction
        if (!leftBlocked && !rightBlocked)
            return (Random.value < 0.5f) ? leftDir : rightDir;
        // If both paths are blocked, return left as a default
        return left;
    }

    bool HasLineOfSight(Transform target)
    {
        // If not obstacle layer is detected, assume we have line of sight and return true
        if (obstacleLayer.value == 0) return true;

        Vector2 origin = rb.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float dist = Vector2.Distance(origin, target.position);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, obstacleLayer);
        return hit.collider == null;
    }

    void UpdateAnimator(Vector2 velocity)
    {
        // If no animator assigned return early to avoid null reference errors
        if (!anim) return;
        // Calculate the speed and direction of movement to update the animator parameters
        float speed = velocity.magnitude;
        anim.SetFloat("Speed", speed);
        // Check if speed is low enough to consider the zombie idle, if so return early
        // This prevents zombie from snapping back to default facing direction
        if (speed < 0.01f)
            return;
        // Calculate the direction of movement
        Vector2 dir = velocity.normalized;

        lastMoveDir = dir;

        if (visual != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }
    // function to try and attack the player
    void TryAttack()
    {
        // If no animator is assigned, return early to avoid null reference errors
        if (!anim) return;
        // If the current time is before the next attack time, return early
        if (Time.time < nextAttackTime)
            return;
        // Set the next attack time to the current time plus the attack cooldown
        nextAttackTime = Time.time + attackCooldown;
        // Trigger the attack animation
        anim.SetTrigger("Attack");
        // Play the attack sound
        DealDamage();
        // If no audio source is assigned, return early to avoid null reference errors
        if (audioSource != null && attackClip != null)
        {
            audioSource.PlayOneShot(attackClip);
        }
}
    void DealDamage()
    {
        // If no attack point is assigned, return early to avoid null reference errors
        if (!attackPoint) return;
        // Keep attack point a fixed distance in front of the zombie based on the last move direction
        // This ensures the attack hitbox is positioned correctly even if the zombie is not currently 
        // moving
        Vector2 offset = lastMoveDir.normalized * 0.35f;
        attackPoint.localPosition = offset;
        // Perform a circle overlap at the attack point to detect if player is within
        // the attack hit radius
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, hitRadius, playerHitLayer);
        // If we don't hit anything, return early
        if (hit == null) return;
        // If we hit the player, get their health component and apply damage
        var health = hit.GetComponent<PlayerHealth>();
        // If the player has a health component, call the TakeDamage method to apply damage
        if (health != null)
            health.TakeDamage(attackDamage);
    }

    void UpdateAnimatorFacing(Vector2 dir)
    { 
        // Check if the direction vector is significant enough to update the animator parameters
        if (dir.sqrMagnitude < 0.0001f) return;

        lastMoveDir = dir.normalized;

        if (visual != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }
    // This method is used for debugging purposes to visualize various radius's
    // such as detection radius, lose radius, and attack git radius
    // Use only in the editor
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