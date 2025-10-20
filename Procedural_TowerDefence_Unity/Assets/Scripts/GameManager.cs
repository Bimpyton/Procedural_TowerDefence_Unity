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
}