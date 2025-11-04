using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BossProfile", menuName = "TowerDefense/BossProfile")]
public class BossProfile : ScriptableObject
{
    [Header("----- VISUALS -----")]
    public string profileName;
    public Material bodyMaterial;
    public Material beakMaterial;
    public Sprite bossIcon;

    [Header("----- MULTIPLIERS -----")]
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;

    [Header("----- TARGET PRIORITY -----")]
    public TargetPriorityMode targetPriorityMode = TargetPriorityMode.Furthest;

    [Header("----- REWARDS -----")]
    public int deathValue = 100;
    public int deathXP = 100;
}