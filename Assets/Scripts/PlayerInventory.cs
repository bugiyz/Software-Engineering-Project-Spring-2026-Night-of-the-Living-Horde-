using UnityEngine;
using UnityEngine.UI;
// This script manages the player's inventory of energy drinks.
public class PlayerInventory : MonoBehaviour
{
    public int maxEnergyDrinks = 3;
    public int currentEnergyDrinks = 0;

    public PlayerMove playerMove;

    [Header("UI")]
    public Image[] energyDrinkIcons;

    void Awake()
    {
        // Initialize the player move component.
        if (playerMove == null)
            playerMove = GetComponent<PlayerMove>();
    }

    void Start()
    {
        // Initialize the player's energy drink count.
        UpdateEnergyDrinkUI();
    }

    void Update()
    {
        // Check for the energy drink key.
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Use an energy drink.
            UseEnergyDrink();
        }
    }
    // Function to add an energy drink to the player's inventory.
    public bool AddEnergyDrink()
    {
        if (currentEnergyDrinks >= maxEnergyDrinks)
            return false;

        currentEnergyDrinks++;
        UpdateEnergyDrinkUI();

        Debug.Log("Energy drinks: " + currentEnergyDrinks);
        return true;
    }
    // Function to use an energy drink from the player's inventory.
    void UseEnergyDrink()
    {
        if (currentEnergyDrinks <= 0) return;

        currentEnergyDrinks--;

        if (playerMove != null)
            playerMove.RestoreStaminaFull();

        UpdateEnergyDrinkUI();

        Debug.Log("Used energy drink. Left: " + currentEnergyDrinks);
    }
    // Function to update the energy drink UI.
    void UpdateEnergyDrinkUI()
    {
        for (int i = 0; i < energyDrinkIcons.Length; i++)
        {
            energyDrinkIcons[i].gameObject.SetActive(i < currentEnergyDrinks);
        }
    }
}