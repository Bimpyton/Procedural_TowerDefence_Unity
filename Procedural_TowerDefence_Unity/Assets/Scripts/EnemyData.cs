using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    public GameObject prefab; // The enemy prefab to instantiate
    public float speed = 5f; // Movement speed
    public int health = 10; // Health points
}