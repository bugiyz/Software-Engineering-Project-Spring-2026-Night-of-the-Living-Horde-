using System.Collections;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
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

    int currentWave = 0;
    int zombiesAlive = 0;
    int zombiesToSpawn = 0;
    bool spawningWave = false;

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        spawningWave = true;

        currentWave++;

        zombiesToSpawn = startingZombies + (currentWave - 1) * zombiesIncreasePerWave;

        Debug.Log("Starting Wave " + currentWave);

        UpdateUI();

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        spawningWave = false;
    }

    void SpawnZombie()
    {
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject z = Instantiate(zombiePrefab, spawn.position, Quaternion.identity);

        ZombieHealth zh = z.GetComponent<ZombieHealth>();

        if (zh != null)
        {
            zh.OnZombieDeath += ZombieDied;
        }

        zombiesAlive++;
        UpdateUI();
    }

    void ZombieDied()
    {
        zombiesAlive--;

        UpdateUI();

        if (zombiesAlive <= 0 && !spawningWave)
        {
            StartCoroutine(WaitForNextWave());
        }
    }

    IEnumerator WaitForNextWave()
    {
        Debug.Log("Wave Cleared!");

        yield return new WaitForSeconds(timeBetweenWaves);

        StartCoroutine(StartNextWave());
    }

    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = "Wave: " + currentWave;

        if (zombiesCountText != null)
            zombiesCountText.text = "Zombies: " + zombiesAlive;
    }
}
