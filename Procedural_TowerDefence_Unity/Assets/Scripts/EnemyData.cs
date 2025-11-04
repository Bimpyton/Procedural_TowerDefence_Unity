using System;
using UnityEngine;

public enum TargetPriorityMode
{
    Closest,
    Furthest
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    public GameObject prefab; // The enemy prefab to instantiate
    public float speed = 5f; // Movement speed
    public float maxHealth = 10f; // Max health points
    public int mainTowerDamage = 10; // Damage to main tower on contact

    [Header("----- ATTACK SETTINGS -----")]
    public float attackSpeed = 1f; // Attacks per second
    public float damage = 1f; // Damage per attack
    public float attackRange = 10f; // Range to attack towers
    public GameObject projectilePrefab; // Projectile prefab
    public float projectileArcHeight = 5f; // Arc height for lobbed projectile
    public Type priorityTargetType; // Type of tower to prioritize by range
    public TargetPriorityMode targetPriorityMode = TargetPriorityMode.Closest; // Prioritize closest or furthest target

    [Header("----- BALANCE -----")]
    public int difficultyValue = 1; // Difficulty rating used by procedural wave generator

    [Header("----- REWARD -----")]
    public int deathValue = 10; // Gold rewarded to player on death
    public int deathXP = 10; // XP rewarded to player on death
}