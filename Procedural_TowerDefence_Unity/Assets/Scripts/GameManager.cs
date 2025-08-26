using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Wave Settings")]
    public List<WaveData> waves = new List<WaveData>();
    private int currentWaveIndex = 0;
    private List<Spawner> spawners = new List<Spawner>();
    private int activeEnemies = 0;
    private bool isWaveInProgress = false;
    [SerializeField] private float timeBetweenWaves = 5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Remove if not needed across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        // Wait briefly to ensure MeshGenerator has spawned spawners
        yield return new WaitForSeconds(0.1f);
        GameObject[] spawnerObjects = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (GameObject spawnerObj in spawnerObjects)
        {
            Spawner spawner = spawnerObj.GetComponent<Spawner>();
            if (spawner != null)
            {
                spawners.Add(spawner);
            }
            else
            {
                
            }
        }

        if (spawners.Count == 0)
        {
            Debug.LogWarning("No spawners found with tag 'Spawner'");
        }

        if (waves.Count == 0)
        {
            Debug.LogWarning("No waves assigned to GameManager");
        }
        else
        {
            
        }

        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves completed");
            return;
        }

        if (isWaveInProgress)
        {
            Debug.Log("Wave in progress, cannot start new wave");
            return;
        }

        isWaveInProgress = true;
        WaveData currentWave = waves[currentWaveIndex];
        Debug.Log($"Starting Wave {currentWaveIndex + 1}");

        foreach (Spawner spawner in spawners)
        {
            StartCoroutine(spawner.SpawnWave(currentWave));
        }

        currentWaveIndex++;
    }

    public void RegisterEnemy()
    {
        activeEnemies++;
        Debug.Log($"Enemy registered. Total active enemies: {activeEnemies}");
    }

    public void UnregisterEnemy()
    {
        activeEnemies--;
        Debug.Log($"Enemy unregistered. Total active enemies: {activeEnemies}");
        if (activeEnemies <= 0 && !isWaveInProgress)
        {
            return;
        }

        if (activeEnemies <= 0)
        {
            isWaveInProgress = false;
            Debug.Log($"Wave completed! Starting next wave in {timeBetweenWaves} seconds...");
            Invoke("StartNextWave", timeBetweenWaves);
        }
    }
}