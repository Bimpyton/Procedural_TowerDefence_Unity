using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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

    [Header("----- UPGRADES -----")]
    public int upgradeLevel = 0;
    [SerializeField] private float upgradeMultiplier = 0.20f;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private float[] upgradeCostMultipliers = {1f, 1.5f, 2f, 2.5f, 5f};
    [SerializeField] private Transform starParent;
    [SerializeField] private GameObject upgradePopupPrefab;

    public SnapPoint snapPoint;

    [Header("----- UI -----")]
    [SerializeField] private HealthBar healthBar;

    [Header("----- ANIMATION -----")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private string shootAnimationTrigger = "Shoot";

    [Header("----- AUDIO -----")]
    public TowerAudio towerAudio;

    void Start()
    {
        towerAudio = FindFirstObjectByType<TowerAudio>();

        if (towerData != null)
        {
            maxHealth = towerData.maxHealth;
            health = maxHealth;
            projectileDamage = towerData.projectileDamage;
            attackRate = towerData.attackRate;
            attackRange = towerData.attackRange;
            projectileArcHeight = towerData.projectileArcHeight;
            projectilePrefab = towerData.projectilePrefab;
            cost = towerData.cost;
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
        // Level 5 passive healing (10% max health per second)
        if (upgradeLevel == 5 && health < maxHealth)
        {
            health += maxHealth * 0.05f * Time.deltaTime;
            if (health > maxHealth) health = maxHealth;
            if (healthBar != null)
            {
                healthBar.SetHealth(health / maxHealth);
            }
        }

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
            towerAudio.PlaySFX(towerAudio.towerShoot);

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

    public float GetNextUpgradeCost()
    {
        if (upgradeLevel >= 5) return -1f;
        return cost * upgradeCostMultipliers[upgradeLevel];
    }

    public void Upgrade()
    {
        if (upgradeLevel >= 5) return;

        upgradeLevel++;
        maxHealth *= (1f + upgradeMultiplier);
        health = maxHealth; // Full heal
        projectileDamage *= (1f + upgradeMultiplier);
        attackRate *= (1f - upgradeMultiplier); 

        if (healthBar != null)
        {
            healthBar.SetHealth(1f);
        }

        AddStar();
        StartCoroutine(ShowUpgradePopups());
    }

    private IEnumerator ShowUpgradePopups()
    {
        List<string> texts = new List<string> { "Health", "Damage", "Attack Speed" };
        bool isLevel5 = upgradeLevel == 5;
        if (isLevel5)
        {
            texts.Add("Passive Healing");
        }
        Color gold = new Color(1f, 0.84f, 0f); // Gold
        float baseSpeed = 1.5f;
        float speedStep = 0.25f; // Each popup is slower by this amount

        for (int i = 0; i < texts.Count; i++)
        {
            if (upgradePopupPrefab != null)
            {
                GameObject popup = Instantiate(upgradePopupPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
                UpgradePopup popupScript = popup.GetComponent<UpgradePopup>();
                if (popupScript != null)
                {
                    popupScript.floatSpeed = baseSpeed - (i * speedStep);
                    if (isLevel5)
                        popupScript.SetText(texts[i], gold);
                    else
                        popupScript.SetText(texts[i]);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void AddStar()
    {
        if (starPrefab == null || starParent == null) return;

        GameObject star = Instantiate(starPrefab, starParent);
        // Layout Group will automatically position it
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
            towerAudio.PlaySFX(towerAudio.towerDeath);
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