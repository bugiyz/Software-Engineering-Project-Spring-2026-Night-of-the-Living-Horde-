using UnityEngine;

// This script is responsible for handling the player's shooting mechanics, 
//including aiming towards the mouse position,

public class PlayerShooter : MonoBehaviour
{
    // Reference to the bullet prefab to instantiate when shooting, the fire point from which
    // the bullet will be fired, and an optional visual transform to use as the origin for aiming
    public Bullet bulletPrefab;
    public Transform firePoint;
    public Transform visual;
    public float fireCooldown = 0.2f;
    // Audio source and clip for shooting sound effects, set in the inspector for easy tweaking
    public AudioSource shootSound;
    public AudioClip shootClip;
// Minimum distance from the player to the mouse position to prevent weird flipping of the aim 
// direction when the mouse is too close to the player, set in the inspector for easy tweaking
    public float minAimDistance = 0.5f;

    private float nextFireTime;

    void Update()
    {
        // Check if player is pressing left mouse button for shooting and if 
        // the current time has reached the next allowed fire time based on the fire cooldown
        // This allows for a consistent rate of fire
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }
    // function to shoot a bullet
    void Shoot()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 dir = mouseWorld - transform.position;
        // If the mouse is too close to the player, use the fire point as the direction
        if (dir.magnitude < minAimDistance)
        {
            // Use the fire point as the direction
            dir = firePoint.position - transform.position;
        }
        // Normalize the direction vector
        dir.Normalize();
        // Fire the bullet in the calculated direction
        Bullet b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        // Set the bullet's direction
        b.Fire(dir);
        // Play the shooting sound
        if (shootSound != null && shootClip != null)
        {
            shootSound.PlayOneShot(shootClip);
        }
    }
}