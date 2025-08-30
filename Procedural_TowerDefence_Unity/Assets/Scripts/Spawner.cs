using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform target;
    public List<Vector2Int> riverPath;

    public IEnumerator SpawnWave(WaveData wave)
    {
        foreach (WaveData.EnemyGroup group in wave.enemyGroups)
        {
            if (group.enemyData == null)
            {
                continue;
            }
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyData);
                yield return new WaitForSeconds(group.spawnDelay);
            }
            yield return new WaitForSeconds(wave.groupDelay);
        }
    }

    void SpawnEnemy(EnemyData enemyData)
    {
        if (enemyData == null || enemyData.prefab == null || target == null || riverPath == null || riverPath.Count == 0) 
        {
            return;
        }

        GameObject enemyObj = Instantiate(enemyData.prefab, transform.position, Quaternion.identity);
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.target = target;
            enemyScript.SetPath(riverPath, transform.parent);
            enemyScript.enemyData = enemyData;
        }
        else
        {
            Debug.LogError($"Enemy prefab {enemyData.prefab.name} is missing the Enemy script");
        }

        GameManager.Instance.RegisterEnemy();
    }
}