using UnityEngine;


// This script is responsible for handling the pickup of energy drinks in the game.
public class EnergyDrinkPickup : MonoBehaviour
{
    // The lifetime of the energy drink pickup before it is destroyed.
    public float lifetime = 10f;

    void Start()
    {
        // Destroy the game object after the specified lifetime.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other object is the player.
        if (!other.CompareTag("Player")) return;
        // Get the player's inventory.
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        // Check if the inventory is found and add the energy drink.
        if (inventory != null && inventory.AddEnergyDrink())
        {
            // Destroy on pickup.
            Destroy(gameObject);
        }
    }
}
