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
    // A velocity variable used by the SmoothDamp function to smoothly interpolate the camera's position
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        // If there is no target assigned, return early to prevent errors
        if (target == null) return;
        // Calculate the target position for the camera based on the target's position, keeping the 
        // camera's z position fixed at -10 to ensure it stays behind the player in a 2D top-down view 
        // Use SmoothDamp to smoothly interpolate the camera's position towards the target position, 
        // creating a smooth following effect
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}