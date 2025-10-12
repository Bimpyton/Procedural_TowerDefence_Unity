using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "TowerDefense/WaveData", order = 2)]
public class WaveData : ScriptableObject
{
    [Header("----- WAVE SETTINGS -----")]
    public float waveDifficulty = 1f; // Difficulty for this wave

    [System.Serializable]
    public class EnemyGroup
    {
        public EnemyData enemyData; // The type of enemy to spawn
        public int count = 1; // Number of this enemy to spawn in the group
        public float spawnDelay = 1f; // Delay between spawning each enemy in the group
    }

    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>(); // List of groups in the wave
    public float groupDelay = 3f; // Delay between different groups
}