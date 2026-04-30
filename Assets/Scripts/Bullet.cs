using UnityEngine;

// This script is responsible for controlling the behavior of the bullet, including its movement,
// lifetime, damage, and interactions with zombies and walls. The bullet will move in a specified
// direction at a constant speed until it either hits an enemy or expires.

public class Bullet : MonoBehaviour
{
    // The speed at which the bullet travels, set in the inspector for easy tweaking
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 10;
    // Reference to the Rigidbody2D component for controlling the bullet's movement
    Rigidbody2D rb;

    void Awake()
    {
        // Get the Rigidbody2D component from the bullet object to control its movement
        rb = GetComponent<Rigidbody2D>();
    }

    public void Fire(Vector2 direction)
    {
        // Set the bullet's velocity in the specified direction multiplied by the speed to make it move
        // in the desired direction at a constant speed, then destroy the bullet after its lifetime 
        // expires
        rb.velocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the bullet collided with a zombie by looking for a ZombieHealth component on the 
        // collided object
        ZombieHealth zh = other.GetComponent<ZombieHealth>();
        if (zh != null)
        {
            // If the collided object has a ZombieHealth component, call the TakeDamage method to apply damage
            zh.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Check if the bullet collided with a wall by checking if the collided object's layer is "Wall" to
        // destroy the bullet on impact with walls to prevent it from passing through them
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
{   
    // If the bullet hits a wall, destroy it to prevent it from passing through the wall
    Destroy(gameObject);
}
    }
}