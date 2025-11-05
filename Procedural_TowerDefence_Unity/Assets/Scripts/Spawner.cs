using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform target;
    public List<Vector2Int> riverPath;

    public void SpawnEnemy(EnemyData enemyData)
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
    }
}