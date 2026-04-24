using UnityEngine;

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
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(meleeKey))
        {
            animator.SetTrigger("BatAttack");
        }
    }

    public void DealMeleeDamage()
{
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
        attackPoint.position,
        attackRange,
        enemyLayer
    );

    foreach (Collider2D enemy in hitEnemies)
    {
        ZombieHealth zombieHealth = enemy.GetComponent<ZombieHealth>();

        if (zombieHealth != null)
        {
            zombieHealth.TakeDamage(meleeDamage);
        }
    }
}

    void OnDrawGizmos()
{
    if (attackPoint == null) return;

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(attackPoint.position, attackRange);
}
}