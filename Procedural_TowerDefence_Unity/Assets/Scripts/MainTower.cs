using UnityEngine;
using TMPro;
using System.Collections;

public class MainTower : MonoBehaviour
{
    [Header("Tower Stats")]

    public float health = 100f;
    public float maxHealth = 100f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float projectileArcHeight = 5f;
    public GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    public float passiveGoldTimer = 3f;
    public int passiveGold = 1;
    public int mainTowerLevel = 1;
    public GameObject upgradeEffectPrefab;

    private float lastAttackTime = 0f;

    [Header("----- UI -----")]
    [SerializeField] private HealthBar healthBar;


    [Header("----- References -----")]
    [SerializeField] private CollapseTower collapseTower;
    [SerializeField] private GameManager gameManager;
    public PlayerManager playerManager;

    void Start()
    {
        health = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetHealth(health / maxHealth);
        }

        collapseTower = GetComponentInChildren<CollapseTower>();
        gameManager = FindFirstObjectByType<GameManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();

        StartCoroutine(PassiveGoldIncome());
    }

    void Update()
    {
        TryAttackEnemy();
    }
    
    
    public float GetNextUpgradeCost()
    {
        return 100 * mainTowerLevel;
    }

    public void UpgradeMainTower()
    {
        mainTowerLevel++;
        passiveGold++;

        SpawnUpgradePopup($"Tower level: {mainTowerLevel}\n\n {passiveGold}G every {passiveGoldTimer} seconds", Color.green);
    }

    public void SpawnUpgradePopup(string text, Color? color = null)
    {
        if (upgradeEffectPrefab != null)
        {
            GameObject popupObj = Instantiate(upgradeEffectPrefab, transform.position + Vector3.up * 5f, Quaternion.identity);
            UpgradePopup popup = popupObj.GetComponent<UpgradePopup>();
            if (popup != null)
            {
                popup.SetText(text, color);
            }
        }
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
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
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
            if (healthBar != null)
            {
                Destroy(healthBar.gameObject);
            }
            health = 0;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Main Tower destroyed!");
        if (collapseTower != null)
        {
            collapseTower.ExplodeTower();
        }
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    //add gold per second
    public IEnumerator PassiveGoldIncome()
    {
        while (true)
        {
            yield return new WaitForSeconds(passiveGoldTimer);
            if (playerManager != null)
            {
                playerManager.AddGold(passiveGold);
            }
        }
    }

}
