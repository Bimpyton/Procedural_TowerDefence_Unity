using System.Collections;
using System.Collections.Generic;
using NUnit;
using Unity.Properties;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("----- GAME SETTINGS -----")]
    [SerializeField] private int frameRateLimit = 60;

    [Header("----- WAVE MANAGER -----")]
    [SerializeField] private WaveManager waveManager;

    [Header("----- FINAL STATS-----")]
    public int totalWavesCompleted = 0;
    public int totalEnemiesDefeated = 0;
    public int totalTowersBuilt = 0;
    public int totalUpgradesPurchased = 0;
    public int totalGoldEarned = 0;
    public int finalScore = 0;


    void Awake()
    {
        Application.targetFrameRate = frameRateLimit;
    }

    void Start()
    {
        if (waveManager == null)
        {
            Debug.LogWarning("WaveManager reference not assigned in GameManager!");
        }
    }

    public int GetCurrentWaveIndex()
    {
        if (waveManager != null)
            return waveManager.currentWaveNumber;
        return -1;
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        StartCoroutine(SlowTimeAndEndGame());
    }

    IEnumerator SlowTimeAndEndGame()
    {
        float duration = 1f;
        float startTime = Time.time;
        float startTimeScale = Time.timeScale;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            Time.timeScale = Mathf.Lerp(startTimeScale, 0f, t);
            yield return null;
        }

        Time.timeScale = 0f;
        Debug.Log("Final Game Over State Reached.");
    }

    public void CalculateFinalScore()
    {
        finalScore = (totalWavesCompleted * 100) + (totalEnemiesDefeated * 10) + totalGoldEarned;
        Debug.Log("Final Score: " + finalScore);
    }

    public void AddGoldEarned(int amount)
    {
        totalGoldEarned += amount;
    }
    public void AddWavesCompleted()
    {
        totalWavesCompleted++;
    }

    public void AddEnemiesDefeated()
    {
        totalEnemiesDefeated++;
    }
    public void AddTowersBuilt()
    {
        totalTowersBuilt++;
    }
    public void AddUpgradesPurchased()
    {
        totalUpgradesPurchased++;
    }
}