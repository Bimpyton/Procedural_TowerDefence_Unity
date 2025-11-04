using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Enemy))]
public class Boss : MonoBehaviour
{
    [Header("----- PROFILES -----")]
    public BossProfile[] profiles;
    [SerializeField] private Image bossIcon;

    [Header("----- CHILD OBJECT & MATERIAL INDICES -----")]
    public string childObjectName = "DuckBoss";
    public int bodyMaterialIndex = 0;
    public int beakMaterialIndex = 1;

    private Enemy enemy;
    private BossProfile selectedProfile;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        selectedProfile = profiles[Random.Range(0, profiles.Length)];
    }

    void Start()
    {
        ApplyProfile();
    }

    void ApplyProfile()
    {
        // MATERIALS
        var renderer = transform.Find(childObjectName)?.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var mats = renderer.materials;
            if (bodyMaterialIndex < mats.Length) mats[bodyMaterialIndex] = selectedProfile.bodyMaterial;
            if (beakMaterialIndex < mats.Length) mats[beakMaterialIndex] = selectedProfile.beakMaterial;
            renderer.materials = mats;
        }

        // ICON
        if (bossIcon != null && selectedProfile.bossIcon != null)
        {
            bossIcon.sprite = selectedProfile.bossIcon;
            bossIcon.enabled = true; 
        }

        // STATS
        enemy.speed *= selectedProfile.speedMultiplier;
        enemy.maxHealth *= selectedProfile.healthMultiplier;
        enemy.health = enemy.maxHealth;
        enemy.damage *= selectedProfile.damageMultiplier;
        enemy.attackSpeed *= selectedProfile.attackSpeedMultiplier;

        // TARGET PRIORITY
        enemy.targetPriorityMode = selectedProfile.targetPriorityMode;

        // REWARDS
        enemy.deathValue = selectedProfile.deathValue;
        enemy.deathXP = selectedProfile.deathXP;

        // NAME
        name = $"Boss_{selectedProfile.profileName}";
        Debug.Log($"BOSS: {selectedProfile.profileName} | HP: {enemy.maxHealth} | Speed: {enemy.speed} | Dmg: {enemy.damage}");
    }
}