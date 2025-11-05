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
    [SerializeField] private int killedDifficultyValue = 0;
    [SerializeField] private int totalDRSpawned = 0;  // Track total spawned DR
    [SerializeField] private int targetDR = 0;
    
    private bool isWaveInProgress = false;
    [SerializeField] private float nextWaveCountdown = 5f;

    public bool canSpawn = true;
    private UIManager uiManager;

    [Header("----- GAME MANAGER -----")]
    [SerializeField] private GameManager gameManager;


    void Start()
    {
        uiManager = Object.FindFirstObjectByType<UIManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        StartCoroutine(SetupWaves());
    }
    void Update()
    {
        if(totalDRSpawned >= targetDR)
        {
            canSpawn = false;
        }
        else
        {
            canSpawn = true;
        }
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
        // Resetting all integer trackers
        killedDifficultyValue = 0;
        totalDRSpawned = 0;
        targetDR = 0;

        WaveData currentWaveData = null;
        
        if (currentWaveNumber % 5 == 0)
        {
            WaveData bossWave = bossWaves.Count > 0 ? bossWaves[Random.Range(0, bossWaves.Count)] : null;
            Debug.Log($"Starting Boss Wave {currentWaveNumber}");
            currentWaveData = bossWave;
        }
        else
        {
            // Initial target is calculated as a float, then immediately cast to int
            float initialTargetDR = currentWaveNumber * 10;
            if (regularWaves.Count == 0)
            {
                Debug.LogWarning("No regular waves assigned to WaveManager");
                isWaveInProgress = false;
                return;
            }
            WaveData selectedWave = regularWaves[Random.Range(0, regularWaves.Count)];
            Debug.Log($"Starting Wave {currentWaveNumber} (Initial Target DR: {initialTargetDR})");
            WaveData runtimeWave = selectedWave;
            
            if (selectedWave.procedural)
            {
                EnsureAvailableEnemyTypes();
                // Pass the initial float target DR to procedural generation
                runtimeWave = GenerateProceduralWave(initialTargetDR);
                if (runtimeWave == null)
                {
                    Debug.LogWarning("Procedural generation failed; aborting wave.");
                    isWaveInProgress = false;
                    return;
                }
                // Assign the final, generated DR to the integer field
                targetDR = (int)runtimeWave.waveDifficulty; 
            }
            else
            {
                // For non-procedural waves, sum the DR of the fixed wave setup
                int calculatedTargetDR = 0;
                if (runtimeWave != null && runtimeWave.enemyGroups != null)
                {
                    foreach (var group in runtimeWave.enemyGroups)
                    {
                        if (group != null && group.enemyData != null)
                            calculatedTargetDR += group.enemyData.difficultyValue * group.count;
                    }
                }
                targetDR = calculatedTargetDR;
            }
            currentWaveData = runtimeWave;
            Debug.Log($"Actual Wave {currentWaveNumber} Target DR: {targetDR}");
        }

        // If boss wave, set targetDR to sum of all enemy difficultyValues in the wave
        if (currentWaveNumber % 5 == 0 && currentWaveData != null && currentWaveData.enemyGroups != null)
        {
            int calculatedTargetDR = 0;
            foreach (var group in currentWaveData.enemyGroups)
            {
                if (group != null && group.enemyData != null)
                    calculatedTargetDR += group.enemyData.difficultyValue * group.count;
            }
            targetDR = calculatedTargetDR;
            Debug.Log($"Actual Boss Wave {currentWaveNumber} Target DR: {targetDR}");
        }

        // Start spawning
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
            
            yield return new WaitForSeconds(wave.groupDelay);

            for (int i = 0; i < group.count; i++)
            {
                int enemyDR = group.enemyData.difficultyValue;

                if (totalDRSpawned + enemyDR > targetDR)
                {
                    Debug.LogWarning($"Spawn budget reached for wave {currentWaveNumber}. Cannot spawn {enemyDR} DR enemy. Total DR: {totalDRSpawned}/{targetDR}");
                    Debug.Log($"Wave {currentWaveNumber} spawning ABORTED. Total spawned DR: {totalDRSpawned}/{targetDR}");
                    yield break; 
                }

                // Update the total DR before the spawn call
                totalDRSpawned += enemyDR;
                Debug.Log($"Enemy spawned [+{enemyDR} DR]. Total Spawned: {totalDRSpawned}/{targetDR} | Killed: {killedDifficultyValue}");


                // Pick a random spawner
                if (spawners.Count == 0 && canSpawn) yield break;
                Spawner chosenSpawner = spawners[Random.Range(0, spawners.Count)];
                chosenSpawner.SpawnEnemy(group.enemyData);
                
                yield return new WaitForSeconds(group.spawnDelay);
            }
        }
        
        Debug.Log($"Wave {currentWaveNumber} spawning COMPLETE. Total spawned DR: {totalDRSpawned}/{targetDR}");
    }

    // Called on EVERY death
    public void UnregisterEnemy(int difficultyValue)
    {
        killedDifficultyValue += difficultyValue;
        Debug.Log($"Enemy killed [+{difficultyValue} DR]. Progress: {killedDifficultyValue}/{targetDR} | Remaining: {targetDR - killedDifficultyValue}");

        // Perfect integer comparison for wave completion
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
            gameManager.AddWavesCompleted();
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
        // Use integer math throughout procedural generation
        int remaining = Mathf.Max(0, Mathf.RoundToInt(targetDR));
        if (remaining <= 0) return null;

        List<EnemyData> pool = new List<EnemyData>(availableEnemyTypes);
        if (pool.Count == 0) return null;

        WaveData runtime = ScriptableObject.CreateInstance<WaveData>();
        runtime.waveDifficulty = targetDR; 
        runtime.procedural = false;
        runtime.enemyGroups = new List<WaveData.EnemyGroup>();
        
        int totalDRGenerated = 0; 

        while (remaining > 0)
        {
            // Candidates are enemies whose difficulty is <= the remaining budget
            List<EnemyData> candidates = pool.FindAll(e => e != null && e.difficultyValue <= remaining);
            if (candidates.Count == 0) break;

            EnemyData chosen = candidates[Random.Range(0, candidates.Count)];
            
            // Calculate max count that fits the *current remaining* budget
            int maxCountForRemaining = Mathf.Max(1, remaining / Mathf.Max(1, chosen.difficultyValue));
            
            // Randomly choose a count, but never exceed the count that fits the remaining budget!
            int count = Random.Range(1, Mathf.Min(maxCountForRemaining, 5) + 1);
            int used = chosen.difficultyValue * count;

            WaveData.EnemyGroup group = new WaveData.EnemyGroup();
            group.enemyData = chosen;
            group.count = count;
            group.spawnDelay = Random.Range(1f, 3f);

            runtime.enemyGroups.Add(group);
            remaining -= used;
            totalDRGenerated += used;
        }
        
        // Update the waveDifficulty to the actual, precise integer value of enemies generated
        runtime.waveDifficulty = totalDRGenerated;

        return runtime;
    }
}