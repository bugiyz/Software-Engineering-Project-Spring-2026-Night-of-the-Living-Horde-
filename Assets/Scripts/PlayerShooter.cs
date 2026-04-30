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

    void Shoot()
    {
        // Get the mouse position in world coordinates to determine the direction to shoot
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        // Use the visual transform for aiming
        Transform aimOrigin = visual != null ? visual : transform;
        // Calculate the direction from the aim origin to the mouse position for shooting
        Vector2 dir = (mouseWorld - aimOrigin.position);

        // Prevent weird flipping when mouse is too close to player
        if (dir.magnitude < minAimDistance)
        {
            dir = firePoint.up; // use current forward direction instead
        }
        // Normalize the direction vector to ensure consistent bullet speed regardless of distance 
        // to mouse
        else
        {
            dir.Normalize();
        }
        // Instantiate a bullet at the fire point position and call its Fire method to set its velocity
        Bullet b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        b.Fire(dir);
        // Play the shooting sound effect
        if (shootSound != null && shootClip != null)
        {
            shootSound.PlayOneShot(shootClip);
        }
    }
}