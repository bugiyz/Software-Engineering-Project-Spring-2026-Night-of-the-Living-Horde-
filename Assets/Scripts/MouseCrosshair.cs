using UnityEngine;
// This script is responsible for controlling the mouse crosshair, including its position and rotation 
// based on the mouse position and player position. The crosshair will follow the mouse cursor and 
// rotate to point in the direction of the player.
public class MouseCrosshair : MonoBehaviour
{
    // Reference to the player transform to calculate the direction from the player to the mouse 
    // position for rotating the crosshair to point towards the player
    public Transform player;

    void Update()
    {
        // Get the mouse position in world coordinates and set the crosshair's position to follow the 
        // mouse cursor
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;
        transform.position = mouse;
        // Check player refernece is assigned before calculating direction and rotation to 
        // prevent null reference errors, then calculate the direction from the player to 
        // the mouse position and rotate the crosshair
        if (player != null)
        {
            Vector2 dir = mouse - player.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
}
