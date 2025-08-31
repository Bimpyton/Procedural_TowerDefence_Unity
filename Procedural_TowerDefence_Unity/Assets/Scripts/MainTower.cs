using UnityEngine;


public class MainTower : MonoBehaviour
{
    [Header("Tower Stats")]

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float projectileArcHeight = 5f;
    public GameObject projectilePrefab;

    private float lastAttackTime = 0f;

    [Header("----- UI -----")]
    [SerializeField] private HealthBar healthBar;

    void Start()
    {
        health = maxHealth;
            if (healthBar != null)
            {
                healthBar.SetHealth(health / maxHealth);
            }
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
                projectile.speed = 10f;
                projectile.damage = (int)damage;
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
        Debug.Log("Main Tower destroyed!");
        Destroy(gameObject);
    }
}
