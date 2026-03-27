using System.Collections;
using UnityEngine;
using TMPro;

// This script is responsible for spawning waves of zombies at specified spawn points,
// managing the wave progression, and updating the UI to display the current wave and number of zombies
// The wave spawner increases the number of zombies each wave and heals the player to full health at 
// the start of each wave
public class WaveSpawner : MonoBehaviour
{
    // Reference to the zombie prefab to spawn, and tweaking of zombie starting count 
    // and increases per wave and also reference to UI elements to update wave and zommbie count 
    // in inspector for easy tweaking without code changes
    [Header("Zombie")]
    public GameObject zombiePrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Wave Settings")]
    public int startingZombies = 3;
    public int zombiesIncreasePerWave = 2;
    public float timeBetweenSpawns = 1f;
    public float timeBetweenWaves = 3f;

    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI zombiesCountText;
    // Internal tracking of the current wave number, number of zombies alive, 
    // and number of zombies to spawn in the current wave
    int currentWave = 0;
    int zombiesAlive = 0;
    int zombiesToSpawn = 0;
    bool spawningWave = false;

    void Start()
    {
        // Start the first wave when the game begins
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        // Set the spawningWave flag to true to indicate that a wave is currently being spawned
        spawningWave = true;
        // Increment the current wave number to track progression
        currentWave++;
        // Heal the player to full health at the start of each wave by finding 
        // the PlayerHealth component
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null){
            playerHealth.Heal(100); // Heal player to full health at the start of each wave
        }
        // Calculate the number of zombies to spawn for this wave based on the starting
        // number and the increase per wave, allowing for scaling difficulty as waves progress
        zombiesToSpawn = startingZombies + (currentWave - 1) * zombiesIncreasePerWave;
        // For debugging purposes only
        Debug.Log("Starting Wave " + currentWave);
        // Update the UI to reflect the new wave and update the new zombies being spawned in
        UpdateUI();
        // Spawn the zombies for this wave with a delay between each spawn to create a more dynamic 
        // and less overwhelming experience for the player can be adjusted to vary in difficulty
        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        // Set the spawningWave flag to false to indicate that all zombies for this wave have been spawned
        spawningWave = false;
    }

    void SpawnZombie()
    {
        // Choose a random spawn point from the array of spawn points to add variety to where 
        // zombies appear
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject z = Instantiate(zombiePrefab, spawn.position, Quaternion.identity);
        // Get the ZombieHealth component from the spawned zombie to subscribe to its death event
        ZombieHealth zh = z.GetComponent<ZombieHealth>();
        // If the ZombieHealth component is found, subscribe to the OnZombieDeath event 
        // to track when this zombie dies
        if (zh != null)
        {
            zh.OnZombieDeath += ZombieDied;
        }
        // Increment the count of zombies alive and update the UI to reflect the new count
        zombiesAlive++;
        UpdateUI();
    }

    void ZombieDied()
    {
        // Decrement the count of zombies alive when a zombie dies and update the UI
        zombiesAlive--;

        UpdateUI();
        // If all zombies have been killed and we are not currently spawning a new wave, 
        // start the next wave after a delay
        if (zombiesAlive <= 0 && !spawningWave)
        {
            StartCoroutine(WaitForNextWave());
        }
    }

    IEnumerator WaitForNextWave()
    {
        // For debugging purposes only
        Debug.Log("Wave Cleared!");
        // Wait for the specified time between waves to give the player a brief respite 
        // before the next wave starts
        yield return new WaitForSeconds(timeBetweenWaves);
        // Start the next wave by calling the StartNextWave coroutine
        StartCoroutine(StartNextWave());
    }

    void UpdateUI()
    {
        // Update the wave and zombies count in the UI elements to reflect the cuurent
        // state of the game
        if (waveText != null)
            waveText.text = "Wave: " + currentWave;

        if (zombiesCountText != null)
            zombiesCountText.text = "Zombies: " + zombiesAlive;
    }
}
