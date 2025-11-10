using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class Enemy : MonoBehaviour

    {
        [Header("----- DATA -----")]
        public EnemyData enemyData;
        public TargetPriorityMode targetPriorityMode = TargetPriorityMode.Closest;

        [Header("----- ENEMY STATS -----")]
        public float speed = 1f;
        public float health = 10f;
        public float maxHealth = 10f;
        public int mainTowerDamage = 10;

        [Header("----- REWARD -----")]
        public int deathValue = 10; // Gold rewarded to player on death
        public int deathXP = 10; // XP rewarded to player on death

        [Header("----- ATTACK -----")]
        public float damage = 5f;
        public float attackSpeed = 1f;
        public GameObject projectilePrefab;
        public float attackRange = 10f;
        public float projectileArcHeight = 5f;
        private float lastAttackTime = 0f;

        [Header("----- DIFFICULTY -----")]
        public int difficultyValue = 1;

        [Header("----- UI -----")]
        [SerializeField] private HealthBar healthBar;

        [Header("----- PATHFINDING -----")]
        public Transform target;
        private List<Vector3> waypoints = new List<Vector3>();
        private int currentWaypointIndex = 0;
        private float waypointThreshold = 0.5f;

        [Header("----- DEATH EFFECTS -----")]
        public GameObject deathParticles;
        public float destroyDelay = 0.5f;

    [Header("----- REFERENCES -----")]
    public GameManager gameManager;
    public EnemyAudio enemyAudio;

        void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        enemyAudio = FindFirstObjectByType<EnemyAudio>();
            
            if (enemyData != null)
            {
                speed = enemyData.speed;
                maxHealth = enemyData.maxHealth;
                health = maxHealth;
                mainTowerDamage = enemyData.mainTowerDamage;

                damage = enemyData.damage;
                attackSpeed = enemyData.attackSpeed;
                projectilePrefab = enemyData.projectilePrefab;
                attackRange = enemyData.attackRange;
                projectileArcHeight = enemyData.projectileArcHeight;
                deathValue = enemyData.deathValue;
                deathXP = enemyData.deathXP;
                difficultyValue = enemyData.difficultyValue;
            }
        }

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
            GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
            GameObject selectedTower = null;
            float bestDistance = (targetPriorityMode == TargetPriorityMode.Furthest) ? 0f : Mathf.Infinity;

            foreach (var tower in towers)
            {
                float dist = Vector3.Distance(transform.position, tower.transform.position);
                if (dist > attackRange) continue; // Skip out-of-range

                bool better = (targetPriorityMode == TargetPriorityMode.Furthest)
                    ? dist > bestDistance
                    : dist < bestDistance;

                if (better)
                {
                    bestDistance = dist;
                    selectedTower = tower;
                }
            }

            // UPDATE HEALTH BAR ONCE
            if (healthBar != null)
                healthBar.SetHealth(health / maxHealth);

            // FIRE!
            if (selectedTower != null && projectilePrefab != null)
            {
                var proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
                var projectile = proj.GetComponent<Projectile>();
                if (projectile)
                {
                    enemyAudio.PlaySFX(enemyAudio.enemyShoot);
                    projectile.target = selectedTower.transform;
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
                mainTower.TakeDamage(mainTowerDamage); 
            }
                
                Die();
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
                enemyAudio.PlaySFX(enemyAudio.enemyDeath);
                Die();
            }
        }

        void Die()
        {
            Debug.Log($"Enemy died: {gameObject.name}");

            // Award player gold and XP
            PlayerManager playerManager = UnityEngine.Object.FindFirstObjectByType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.AddGold(deathValue);
                playerManager.AddScore(deathXP); // XP per kill
            }

            // Notify WaveManager
            WaveManager waveManager = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                Debug.Log($"Their DR was {difficultyValue}");
                waveManager.UnregisterEnemy(difficultyValue);
            }

            if (deathParticles)
            {
                Instantiate(deathParticles, transform.position, Quaternion.identity);
            }

            gameManager.AddEnemiesDefeated();
            gameManager.AddGoldEarned(deathValue);

            Destroy(gameObject, destroyDelay);
        }
    }
