using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaveManager : MonoBehaviour
{
    [Header("----- WAVE SETTINGS -----")]
    public List<WaveData> regularWaves = new List<WaveData>(); // Waves with difficulty
    public List<WaveData> bossWaves = new List<WaveData>();    // Boss waves
    public List<EnemyData> availableEnemyTypes = new List<EnemyData>(); // Optional pool for procedural generation
    public int currentWaveNumber = 0; // Start at 0 for wave 1
    private List<Spawner> spawners = new List<Spawner>();
    [SerializeField] private float killedDifficultyValue = 0f;
    [SerializeField] private float totalDRSpawned = 0f;  // Track total spawned DR
    [SerializeField] private float targetDR = 0f;
    private bool isWaveInProgress = false;
    [SerializeField] private float nextWaveCountdown = 5f;
    private UIManager uiManager;

    void Awake()
    {
        
    }

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
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
        currentWaveNumber++;

        if (isWaveInProgress)
        {
            return;
        }

        isWaveInProgress = true;
        killedDifficultyValue = 0f;
        totalDRSpawned = 0f;  // RESET: Total spawned for this wave
        targetDR = 0f;

        WaveData currentWaveData = null;
        // Boss wave every 5th wave
        if (currentWaveNumber % 5 == 0)
        {
            WaveData bossWave = bossWaves.Count > 0 ? bossWaves[Random.Range(0, bossWaves.Count)] : null;
            Debug.Log($"Starting Boss Wave {currentWaveNumber}");
            currentWaveData = bossWave;
        }
        else
        {
            targetDR = currentWaveNumber * 10;
            if (regularWaves.Count == 0)
            {
                Debug.LogWarning("No regular waves assigned to WaveManager");
                isWaveInProgress = false;
                return;
            }
            WaveData selectedWave = regularWaves[Random.Range(0, regularWaves.Count)];
            Debug.Log($"Starting Wave {currentWaveNumber} (Target DR: {targetDR})");
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
            currentWaveData = runtimeWave;
        }

        // If boss wave, set targetDR to sum of all enemy difficultyValues in the wave
        if (currentWaveNumber % 5 == 0 && currentWaveData != null && currentWaveData.enemyGroups != null)
        {
            targetDR = 0f;
            foreach (var group in currentWaveData.enemyGroups)
            {
                if (group != null && group.enemyData != null)
                    targetDR += group.enemyData.difficultyValue * group.count;
            }
        }

        // Start spawning: spawn each enemy at a random spawner
        if (currentWaveData != null)
        {
            StartCoroutine(SpawnWaveAtRandomSpawners(currentWaveData));
        }
    }

    private IEnumerator SpawnWaveAtRandomSpawners(WaveData wave)
    {
        foreach (WaveData.EnemyGroup group in wave.enemyGroups)
        {
            if (group.enemyData == null)
            {
                continue;
            }
            
            // STOP SPAWNING if we've reached target DR
            if (totalDRSpawned >= targetDR)
            {
                Debug.Log($"✅ All enemies spawned for wave {currentWaveNumber}! Total DR: {totalDRSpawned}/{targetDR}");
                yield break;
            }
            
            for (int i = 0; i < group.count; i++)
            {
                // STOP SPAWNING if we've reached target DR
                if (totalDRSpawned >= targetDR)
                {
                    Debug.Log($"✅ Spawn limit reached for wave {currentWaveNumber}. Total DR: {totalDRSpawned}/{targetDR}");
                    yield break;
                }

                // Pick a random spawner
                if (spawners.Count == 0) yield break;
                Spawner chosenSpawner = spawners[Random.Range(0, spawners.Count)];
                chosenSpawner.SpawnEnemy(group.enemyData);
                yield return new WaitForSeconds(group.spawnDelay);
            }
            yield return new WaitForSeconds(wave.groupDelay);
        }
        
        Debug.Log($"✅ Wave {currentWaveNumber} spawning COMPLETE. Total spawned DR: {totalDRSpawned}/{targetDR}");
    }

    // Called on EVERY spawn
    public void EnemySpawned(int difficultyValue)
    {
        totalDRSpawned += difficultyValue;
        Debug.Log($"Enemy spawned [+{difficultyValue} DR]. Total Spawned: {totalDRSpawned}/{targetDR} | Killed: {killedDifficultyValue}");
    }

    // Called on EVERY death
    public void UnregisterEnemy(int difficultyValue)
    {
        killedDifficultyValue += difficultyValue;
        Debug.Log($"Enemy killed [+{difficultyValue} DR]. Progress: {killedDifficultyValue}/{targetDR} | Remaining: {targetDR - killedDifficultyValue}");

        if (isWaveInProgress && killedDifficultyValue >= targetDR)
        {
            EndWave();
        }
    }

    private void EndWave()
    {
        isWaveInProgress = false;
        Debug.Log($"Wave {currentWaveNumber} COMPLETED! Killed: {killedDifficultyValue}/{targetDR} | Spawned: {totalDRSpawned}");
        if (uiManager != null)
        {
            uiManager.ShowStartNextWaveButton(nextWaveCountdown);
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
            group.spawnDelay = Random.Range(1f, 3f);

            runtime.enemyGroups.Add(group);
            remaining -= used;
        }

        if (remaining > 0)
        {
            EnemyData smallest = pool.Where(e => e != null).OrderBy(e => e.difficultyValue).FirstOrDefault();
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