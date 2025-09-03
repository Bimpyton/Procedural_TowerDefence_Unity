using UnityEngine;
using System.Collections;

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
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float projectileArcHeight = 5f;
    [SerializeField] private GameObject projectilePrefab;
    private float lastAttackTime = 0f;
    [SerializeField] private float cost = 50f;

    public SnapPoint snapPoint;

    [Header("----- UI -----")]
    [SerializeField] private HealthBar healthBar;

    void Start()
    {
        if (towerData != null)
        {
            maxHealth = towerData.maxHealth;
            health = maxHealth;
            projectileDamage = towerData.projectileDamage;
            attackSpeed = towerData.attackSpeed;
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
            // Improved spring formula: damped sine wave for bouncier effect
            float value = 1f - Mathf.Exp(-springiness * progress) * Mathf.Cos(progress * Mathf.PI * springiness);
            target.localScale = Vector3.LerpUnclamped(startScale, finalScale, value);
            yield return null;
        }
        target.localScale = finalScale; // Snap to final
    }

    void Update()
    {
        TryAttackEnemy();
    }

    void TryAttackEnemy()
    {
        if (Time.time - lastAttackTime >= 1f / attackSpeed)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    void Attack()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float minDist = Mathf.Infinity;
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < attackRange && dist < minDist)
            {
                minDist = dist;
                nearestEnemy = enemy;
            }
        }
        if (nearestEnemy != null && projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.target = nearestEnemy.transform;
                projectile.arcHeight = projectileArcHeight;
                projectile.speed = towerData.projectileSpeed;
                projectile.damage = (int)projectileDamage;
            }
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
        // Handle tower destruction (animation, removal, etc.)
        if (snapPoint != null)
        {
            snapPoint.TowerDestroyed();
        }
        Destroy(gameObject);

        void OnDestroy()
        {
            if (snapPoint != null)
            {
                snapPoint.TowerDestroyed();
            }
        }
    }
}
