using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("----- WAVE SETTINGS -----")]
    public List<WaveData> regularWaves = new List<WaveData>(); // Waves with difficulty
    public List<WaveData> bossWaves = new List<WaveData>();    // Boss waves
    public List<EnemyData> availableEnemyTypes = new List<EnemyData>(); // Optional pool for procedural generation
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
            int targetDR = currentWaveNumber * 5;
            if (regularWaves.Count == 0)
            {
                Debug.LogWarning("No regular waves assigned to WaveManager");
                return;
            }
            WaveData selectedWave = regularWaves[Random.Range(0, regularWaves.Count)];
            Debug.Log($"Starting Wave {currentWaveNumber} (DR {targetDR})");
            WaveData runtimeWave = selectedWave;
            if (selectedWave.procedural)
            {
                EnsureAvailableEnemyTypes();
                runtimeWave = GenerateProceduralWave(targetDR);
                if (runtimeWave == null)
                {
                    Debug.LogWarning("Procedural generation failed; aborting wave.");
                    isWaveInProgress = false;
                    return;
                }
            }
            foreach (Spawner spawner in spawners)
            {
                StartCoroutine(spawner.SpawnWave(runtimeWave));
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

    // Ensure the availableEnemyTypes list is populated from regular waves if not set in inspector
    private void EnsureAvailableEnemyTypes()
    {
        if (availableEnemyTypes != null && availableEnemyTypes.Count > 0) return;
        HashSet<EnemyData> set = new HashSet<EnemyData>();
        foreach (var w in regularWaves)
        {
            if (w == null) continue;
            foreach (var g in w.enemyGroups)
            {
                if (g != null && g.enemyData != null) set.Add(g.enemyData);
            }
        }
        availableEnemyTypes = new List<EnemyData>(set);
    }

    // Generate a runtime WaveData instance whose enemyGroups sum to approximately targetDR using enemy difficulty values
    private WaveData GenerateProceduralWave(float targetDR)
    {
        int remaining = Mathf.Max(0, Mathf.RoundToInt(targetDR));
        if (remaining <= 0) return null;

        List<EnemyData> pool = new List<EnemyData>(availableEnemyTypes);
        if (pool.Count == 0) return null;

        WaveData runtime = ScriptableObject.CreateInstance<WaveData>();
        runtime.waveDifficulty = targetDR;
        runtime.procedural = false;
        runtime.enemyGroups = new List<WaveData.EnemyGroup>();

        while (remaining > 0)
        {
            List<EnemyData> candidates = pool.FindAll(e => e != null && e.difficultyValue <= remaining);
            if (candidates.Count == 0) break;

            EnemyData chosen = candidates[Random.Range(0, candidates.Count)];
            int maxCount = Mathf.Max(1, remaining / Mathf.Max(1, chosen.difficultyValue));
            int count = Random.Range(1, Mathf.Min(maxCount, 5) + 1);
            int used = chosen.difficultyValue * count;

            WaveData.EnemyGroup group = new WaveData.EnemyGroup();
            group.enemyData = chosen;
            group.count = count;
            group.spawnDelay = Random.Range(1f, 2f);

            runtime.enemyGroups.Add(group);
            remaining -= used;
        }

        if (remaining > 0)
        {
            EnemyData smallest = pool[0];
            foreach (var e in pool) if (e.difficultyValue < smallest.difficultyValue) smallest = e;
            if (smallest != null && smallest.difficultyValue > 0)
            {
                int extra = Mathf.CeilToInt((float)remaining / smallest.difficultyValue);
                WaveData.EnemyGroup group = new WaveData.EnemyGroup();
                group.enemyData = smallest;
                group.count = extra;
                group.spawnDelay = 0.5f;
                runtime.enemyGroups.Add(group);
                remaining = 0;
            }
        }

        return runtime;
    }
}
