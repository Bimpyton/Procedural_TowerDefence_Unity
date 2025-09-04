using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("----- WAVE SETTINGS -----")]
    public List<WaveData> waves = new List<WaveData>();
    public int CurrentWaveIndex = 0;
    private List<Spawner> spawners = new List<Spawner>();
    private int activeEnemies = 0;
    private bool isWaveInProgress = false;
    [SerializeField] private float timeBetweenWaves = 5f;

    void Awake()
    {
        // Optionally, validate setup
    }

    void Start()
    {
        StartCoroutine(SetupWaves());
    }

    private IEnumerator SetupWaves()
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
        }

        if (spawners.Count == 0)
        {
            Debug.LogWarning("No spawners found with tag 'Spawner'");
        }

        if (waves.Count == 0)
        {
            Debug.LogWarning("No waves assigned to WaveManager");
        }

        StartNextWave();
    }

    public void StartNextWave()
    {
        if (CurrentWaveIndex >= waves.Count)
        {
            return;
        }

        if (isWaveInProgress)
        {
            return;
        }

        isWaveInProgress = true;
        WaveData currentWave = waves[CurrentWaveIndex];
        Debug.Log($"Starting Wave {CurrentWaveIndex + 1}");

        foreach (Spawner spawner in spawners)
        {
            StartCoroutine(spawner.SpawnWave(currentWave));
        }

        CurrentWaveIndex++;
    }

    public void RegisterEnemy()
    {
        activeEnemies++;
    }

    public void UnregisterEnemy()
    {
        activeEnemies--;
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
