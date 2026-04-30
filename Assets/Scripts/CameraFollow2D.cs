using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for making the camera follow the player smoothly in a 2D top-down game.
// The camera will follow the player's position with a smooth damping effect to create a more polished 
// and professional feel to the camera movement.
public class CameraFollow2D : MonoBehaviour
{
    // The target transform that the camera will follow, typically the player character
    public Transform target;

    public float smoothTime = 0.12f;

    // A velocity variable used by the SmoothDamp function
    private Vector3 velocity = Vector3.zero;

    // NEW: Camera boundary limits (prevents showing outside map)
    [Header("Camera Bounds")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        // If there is no target assigned, return early to prevent errors
        if (target == null) return;

        // Calculate target position (keep camera at z = -10)
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);

        // Smoothly move camera toward target
        Vector3 smoothPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // NEW: Clamp camera position so it does not go outside map bounds
        float clampedX = Mathf.Clamp(smoothPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(smoothPos.y, minY, maxY);

        // Apply final clamped position
        transform.position = new Vector3(clampedX, clampedY, smoothPos.z);
    }
}