using UnityEngine;

// This script handles the player's melee attacks.

public class PlayerMelee : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;

    [Header("Input")]
    public KeyCode meleeKey = KeyCode.Mouse1; // Right click

    [Header("Melee Attack")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int meleeDamage = 25;
    public LayerMask enemyLayer;

    void Awake()
    {
        // Initialize the player's melee attack.
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        // Check for the melee attack key.
        if (Input.GetKeyDown(meleeKey))
        {
            // Perform the melee attack.
            animator.SetTrigger("BatAttack");
        }
    }
    // function to deal melee damage
    public void DealMeleeDamage()
{
    // Check for enemies in the attack range.
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
        attackPoint.position,
        attackRange,
        enemyLayer
    );
    // Damage each enemy hit.
    foreach (Collider2D enemy in hitEnemies)
    {
        // Get the zombie's health component.
        ZombieHealth zombieHealth = enemy.GetComponent<ZombieHealth>();
        // If the zombie has a health component, deal damage to it.
        if (zombieHealth != null)
        {
            zombieHealth.TakeDamage(meleeDamage);
        }
    }
}
    // Draw the attack range in the editor, for visualization purposes.
    void OnDrawGizmos()
{
    if (attackPoint == null) return;

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(attackPoint.position, attackRange);
}
}