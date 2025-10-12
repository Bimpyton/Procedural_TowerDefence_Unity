using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("----- WAVE SETTINGS -----")]
    public List<WaveData> regularWaves = new List<WaveData>(); // Waves with difficulty
    public List<WaveData> bossWaves = new List<WaveData>();    // Boss waves
    public int currentWaveNumber = 1;
    private List<Spawner> spawners = new List<Spawner>();
    private int activeEnemies = 0;
    private bool isWaveInProgress = false;
    [SerializeField] private float timeBetweenWaves = 5f;

    void Awake()
    {
        
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

        if (regularWaves.Count == 0)
        {
            Debug.LogWarning("No regular waves assigned to WaveManager");
        }

        StartNextWave();
    }

    public void StartNextWave()
    {
        if (isWaveInProgress)
        {
            return;
        }

        isWaveInProgress = true;

        // Boss wave every 5th wave
        if (currentWaveNumber % 5 == 0)
        {
            // Select boss wave
            WaveData bossWave = bossWaves.Count > 0 ? bossWaves[Random.Range(0, bossWaves.Count)] : null;
            Debug.Log($"Starting Boss Wave {currentWaveNumber}");
            if (bossWave != null)
            {
                foreach (Spawner spawner in spawners)
                {
                    StartCoroutine(spawner.SpawnWave(bossWave));
                }
            }
            else
            {
                Debug.LogWarning("No boss waves defined!");
            }
        }
        else
        {
            // Determine difficulty based on wave number
            int difficulty = ((currentWaveNumber - 1) / 5) + 1;
            // Filter regular waves by difficulty using float comparison
            List<WaveData> possibleWaves = regularWaves.FindAll(w => Mathf.Approximately(w.waveDifficulty, difficulty));
            if (possibleWaves.Count == 0)
            {
                Debug.LogWarning($"No regular waves found for difficulty {difficulty}");
                return;
            }
            WaveData selectedWave = possibleWaves[Random.Range(0, possibleWaves.Count)];
            Debug.Log($"Starting Wave {currentWaveNumber} (Difficulty {difficulty})");
            foreach (Spawner spawner in spawners)
            {
                StartCoroutine(spawner.SpawnWave(selectedWave));
            }
        }

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
            currentWaveNumber++; // Increment wave number after wave completion
            Debug.Log($"Wave completed! Starting next wave in {timeBetweenWaves} seconds...");
            Invoke("StartNextWave", timeBetweenWaves);
        }
    }

    public int GetCurrentWaveIndex()
    {
        return currentWaveNumber;
    }
}
