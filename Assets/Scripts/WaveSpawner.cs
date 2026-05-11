using System.Collections;
using UnityEngine;
using TMPro;
using Pathfinding;

// This script is responsible for spawning waves of zombies at specified spawn points,
// managing the wave progression, and updating the UI to display the current wave and number of zombies
// The wave spawner increases the number of zombies each wave and heals the player to full health at 
// the start of each wave
public class WaveSpawner : MonoBehaviour
{
    // Reference to the zombie prefab to spawn, and tweaking of zombie starting count 
    // and increases per wave and also reference to UI elements to update wave and zommbie count 
    // in inspector for easy tweaking without code changes
    [Header("Zombie Types")]
    public GameObject basicZombiePrefab;
    public GameObject smartZombiePrefab;

    [Range(0f, 1f)]
    public float startingSmartZombieRatio = 0.5f;

    [Range(0f, 1f)]
    public float smartZombieRatioIncreasePerWave = 0.05f;

    [Range(0f, 1f)]
    public float maxSmartZombieRatio = 0.9f;
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
        if (GameStats.Instance != null)
        {
            GameStats.Instance.SetWave(currentWave);
        }
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
    // function to spawn a single zombie
    void SpawnZombie()
{
    if (spawnPoints.Length == 0) return;

    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

    float smartRatio = GetSmartZombieRatio();

    GameObject prefabToSpawn = Random.value < smartRatio
        ? smartZombiePrefab
        : basicZombiePrefab;

    GameObject zombie = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
    ApplyDifficultyToZombie(zombie);

    ZombieHealth zh = zombie.GetComponent<ZombieHealth>();
    if (zh != null)
    {
        zh.OnZombieDeath += ZombieDied;
    }

    zombiesAlive++;
    UpdateUI();
}
    // function called when a zombie dies
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

    // function to wait for the next wave
    IEnumerator WaitForNextWave()
{
    Debug.Log("Wave Cleared!");

    yield return new WaitForSeconds(timeBetweenWaves);

    StartCoroutine(StartNextWave());
}

    // function to get the ratio of smart zombies
    float GetSmartZombieRatio()
{
    var difficulty = SettingsManager.Instance != null
        ? SettingsManager.Instance.difficulty
        : SettingsManager.Difficulty.Easy;
    // Determine the ratio of smart zombies based on the current difficulty level
    switch (difficulty)
    {
        // Easy difficulty: 20% smart zombies
        case SettingsManager.Difficulty.Easy:
            return 0.20f;
        // Medium difficulty: start with 50% smart zombies and increase by 5% each wave
        case SettingsManager.Difficulty.Normal:
            return Mathf.Clamp(
                0.50f + (currentWave - 1) * 0.05f,
                0.50f,
                0.80f
            );
        // Hard difficulty: start with 70% smart zombies and increase by 5% each wave
        case SettingsManager.Difficulty.Hard:
            return Mathf.Clamp(
                0.70f + (currentWave - 1) * 0.05f,
                0.70f,
                0.95f
            );

        default:
            return 0.20f;
    }
}
    // function to apply difficulty settings to a zombie
    void ApplyDifficultyToZombie(GameObject zombie)
{
    // Apply difficulty settings to the zombie
    var difficulty = SettingsManager.Instance != null
        ? SettingsManager.Instance.difficulty
        : SettingsManager.Difficulty.Easy;
    // Determine the difficulty settings based on the current difficulty level
    int damage = 5;
    // Set speed multiplier based on the difficulty level
    float speedMultiplier = 0.85f;
    // Apply the speed multiplier to the zombie
    switch (difficulty)
    {
        // Easy difficulty: 5 damage, 0.85 speed
        case SettingsManager.Difficulty.Easy:
            damage = 5;
            speedMultiplier = 0.85f;
            break;
        // Medium difficulty: 10 damage, 1.15 speed
        case SettingsManager.Difficulty.Normal:
            damage = 10;
            speedMultiplier = 1.15f;
            break;
        // Hard difficulty: 15 damage, 1.25 speed
        case SettingsManager.Difficulty.Hard:
            damage = 20;
            speedMultiplier = 1.25f;
            break;
    }

    // Apply the difficulty settings to the zombie's AI components
    ZombieAI basicAI = zombie.GetComponent<ZombieAI>();
    // If the zombie has a basic AI component, apply the difficulty settings to it
    if (basicAI != null)
    {
        basicAI.attackDamage = damage;
        basicAI.chaseSpeed *= speedMultiplier;
        basicAI.roamSpeed *= speedMultiplier;
    }
    // If the zombie has a pathfinding AI component, apply the difficulty settings to it
    ZombiePathfindingAI smartAI = zombie.GetComponent<ZombiePathfindingAI>();

    if (smartAI != null)
    {
        smartAI.attackDamage = damage;

        AIPath path = zombie.GetComponent<AIPath>();

        if (path != null)
        {
            path.maxSpeed *= speedMultiplier;
        }
    }
}
    // function to update the UI
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
