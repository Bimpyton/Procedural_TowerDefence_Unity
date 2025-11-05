using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    [Header("----- SPRING EFFECT -----")]
    [SerializeField] private float springiness = 4f;
    [SerializeField] private float springTime = 1f;
    [SerializeField] private float startScale = 0.01f;
    [Header("----- DATA -----")]
    public TowerData towerData;

    [Header("----- TOWER STATS -----")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float projectileDamage = 20f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float projectileArcHeight = 5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool isAttacking = false;
    public float cost = 50f;

    public SnapPoint snapPoint;

    [Header("----- UI -----")]
    [SerializeField] private HealthBar healthBar;

    [Header("----- ANIMATION -----")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDuration = 2f; 
    [SerializeField] private string shootAnimationTrigger = "Shoot";

    void Start()
    {
        if (towerData != null)
        {
            maxHealth = towerData.maxHealth;
            health = maxHealth;
            projectileDamage = towerData.projectileDamage;
            attackRate = towerData.attackRate;
            attackRange = towerData.attackRange;
            projectileArcHeight = towerData.projectileArcHeight;
            projectilePrefab = towerData.projectilePrefab;
        }
        if (healthBar != null)
        {
            healthBar.SetHealth(health / maxHealth);
        }

        // Spring effect on placement
        transform.localScale = Vector3.one * startScale;
        StartCoroutine(SpringScale(transform, Vector3.one, springTime, springiness));
    }

    private IEnumerator SpringScale(Transform target, Vector3 finalScale, float duration, float springiness)
    {
        float time = 0f;
        Vector3 startScale = target.localScale;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float value = 1f - Mathf.Exp(-springiness * progress) * Mathf.Cos(progress * Mathf.PI * springiness);
            target.localScale = Vector3.LerpUnclamped(startScale, finalScale, value);
            yield return null;
        }
        target.localScale = finalScale; // Snap to final
    }

    void Update()
    {
        if (canAttack && !isAttacking)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        GameObject target = FindTarget();
        if (target != null)
        {
            canAttack = false;
            // Play animation
            if (animator != null && !string.IsNullOrEmpty(shootAnimationTrigger))
            {
                animator.SetTrigger(shootAnimationTrigger);
            }

            // Wait for animation to finish
            yield return new WaitForSeconds(animationDuration);

            // Shoot projectile
            yield return Attack(target);

            // Wait for cooldown
            yield return new WaitForSeconds(attackRate);

            canAttack = true;
        }
        isAttacking = false;
    }

        // Finds the best target according to priority
        private GameObject FindTarget()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject selectedEnemy = null;
            List<GameObject> inRangeEnemies = new List<GameObject>();
            float selectedDist = (towerData != null && towerData.targetPriorityMode == TowerData.TargetPriorityMode.Furthest) ? -Mathf.Infinity : Mathf.Infinity;
            foreach (var enemy in enemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < attackRange)
                {
                    inRangeEnemies.Add(enemy);
                    if (towerData != null && towerData.targetPriorityMode == TowerData.TargetPriorityMode.Furthest)
                    {
                        if (dist > selectedDist)
                        {
                            selectedDist = dist;
                            selectedEnemy = enemy;
                        }
                    }
                    else if (towerData != null && towerData.targetPriorityMode == TowerData.TargetPriorityMode.Closest)
                    {
                        if (dist < selectedDist)
                        {
                            selectedDist = dist;
                            selectedEnemy = enemy;
                        }
                    }
                }
            }
            if (towerData != null && towerData.targetPriorityMode == TowerData.TargetPriorityMode.Random && inRangeEnemies.Count > 0)
            {
                selectedEnemy = inRangeEnemies[UnityEngine.Random.Range(0, inRangeEnemies.Count)];
            }
            return selectedEnemy;
        }

        // Attack the given target
        private IEnumerator Attack(GameObject selectedEnemy)
        {
            if (selectedEnemy != null && projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
                Projectile projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.target = selectedEnemy.transform;
                    projectile.arcHeight = projectileArcHeight;
                    projectile.speed = towerData.projectileSpeed;
                    projectile.damage = (int)projectileDamage;
                }
            }
            yield return null;
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
        // Tower destruction
        if (snapPoint != null)
        {
            snapPoint.TowerDestroyed();
        }
        Destroy(gameObject);

    }
}
