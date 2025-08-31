using UnityEngine;
using System.Collections.Generic;
using System;

public class Enemy : MonoBehaviour
{
    [Header("----- DATA -----")]
    public EnemyData enemyData;

    [Header("----- ENEMY STATS -----")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float health = 10f;
    [SerializeField] private float maxHealth = 10f;
    [Header("----- REWARD -----")]
    public int deathValue = 10; // Gold rewarded to player on death
    public int deathXP = 10; // XP rewarded to player on death

    [Header("----- ATTACK -----")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float projectileArcHeight = 5f;
    private float lastAttackTime = 0f;

        [Header("----- UI -----")]
        [SerializeField] private HealthBar healthBar;

    void Start()
    {
        if (enemyData != null)
        {
            speed = enemyData.speed;
            maxHealth = enemyData.maxHealth;

            health = maxHealth;

            damage = enemyData.damage;
            attackSpeed = enemyData.attackSpeed;
            projectilePrefab = enemyData.projectilePrefab;
            attackRange = enemyData.attackRange;
            projectileArcHeight = enemyData.projectileArcHeight;
            deathValue = enemyData.deathValue;
            deathXP = enemyData.deathXP;
        }
    }

    [Header("----- PATHFINDING -----")]
    
    public Transform target;
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private float waypointThreshold = 0.5f;

    public void SetPath(List<Vector2Int> riverPath, Transform meshGeneratorTransform)
    {
        MeshGenerator mg = meshGeneratorTransform.GetComponent<MeshGenerator>();
        if (mg == null)
        {
            Debug.LogError($"MeshGenerator component not found on {meshGeneratorTransform.name}!");
            return;
        }
        waypoints.Clear();
        foreach (Vector2Int point in riverPath)
        {
            int idx = point.y * (mg.xSize + 1) + point.x;
            Vector3 localPos = mg.vertices[idx];
            Vector3 worldPos = meshGeneratorTransform.TransformPoint(localPos);
            worldPos.y += 1f;
            waypoints.Add(worldPos);
        }
    }

    void Update()
    {
        if (waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
        {
            Debug.LogWarning($"No waypoints or reached end for enemy: {gameObject.name}");
            return;
        }

        Vector3 targetPos = waypoints[currentWaypointIndex];
        transform.LookAt(targetPos);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < waypointThreshold)
        {
            currentWaypointIndex++;
        }

        // Attack towers in range
        TryAttackTower();
    }

    void Attack()
    {
        // Find nearest tower in range
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        GameObject nearestTower = null;
        float minDist = Mathf.Infinity;
        foreach (var tower in towers)
        {
            float dist = Vector3.Distance(transform.position, tower.transform.position);
            if (dist < attackRange && dist < minDist)
            {
                minDist = dist;
                nearestTower = tower;
            if (healthBar != null)
            {
                healthBar.SetHealth(health / maxHealth);
            }
            }
        }
        if (nearestTower != null && projectilePrefab != null)
        {
            // Shoot projectile
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.target = nearestTower.transform;
                projectile.arcHeight = projectileArcHeight;
                projectile.speed = 10f;
                projectile.damage = (int)damage;
            }
        }
    }

        void TryAttackTower()
        {
            if (Time.time - lastAttackTime >= 1f / attackSpeed)
            {
                lastAttackTime = Time.time;
                Attack();
            }
        }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainTower"))
        {
            Debug.Log($"Enemy hit Main Tower");
            MainTower mainTower = other.GetComponent<MainTower>();
            if (mainTower != null)
            {
                mainTower.TakeDamage(10); // Flat 10 damage to main tower
            }
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
            if (healthBar != null)
            {
                healthBar.SetHealth(health / maxHealth);
            }
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Award player gold and XP
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.AddGold(deathValue);
            playerManager.AddScore(deathXP); // XP per kill
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterEnemy();
        }
    }
}