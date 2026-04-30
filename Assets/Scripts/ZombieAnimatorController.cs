using UnityEngine;

// This script is responsible for controlling the zombie's animator parameters based on its movement
public class ZombieAnimatorController : MonoBehaviour
{
    // Reference to the Animator component to control the zombie's animations
    Animator animator;
    Rigidbody2D rb;

    void Start()
    {
        // Get the animator and rigidbody components from the parent object
        // to control the animations based on the zombie's movement
        animator = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        // Get the current velocity of the zombie from its Rigidbody2D component
        Vector2 velocity = rb.velocity;
        // Update the animator parameters for movement direction based on the velocity
        animator.SetFloat("MoveX", velocity.x);
        animator.SetFloat("MoveY", velocity.y);
    }
}