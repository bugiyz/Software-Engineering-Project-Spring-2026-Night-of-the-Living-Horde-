using UnityEngine;

public class ZombieAnimatorController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 velocity = rb.velocity;

        animator.SetFloat("MoveX", velocity.x);
        animator.SetFloat("MoveY", velocity.y);
    }
}