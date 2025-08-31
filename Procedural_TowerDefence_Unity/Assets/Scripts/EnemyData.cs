using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    public GameObject prefab; // The enemy prefab to instantiate
    public float speed = 5f; // Movement speed
    public float maxHealth = 10f; // Max health points

    [Header("----- ATTACK SETTINGS -----")]
    public float attackSpeed = 1f; // Attacks per second
    public float damage = 1f; // Damage per attack
    public float attackRange = 10f; // Range to attack towers
    public GameObject projectilePrefab; // Projectile prefab
    public float projectileArcHeight = 5f; // Arc height for lobbed projectile

    [Header("----- REWARD -----")]
    public int deathValue = 10; // Gold rewarded to player on death
    public int deathXP = 10; // XP rewarded to player on death
}