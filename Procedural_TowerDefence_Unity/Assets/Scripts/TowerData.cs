using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefense/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("----- PROJECTILE -----")]
	public float projectileSpeed = 10f;
	public float projectileDamage = 20f;
	public GameObject projectilePrefab;

	[Header("----- TOWER STATS -----")]
	public float maxHealth = 100f;
	public float damage = 20f;
	public float attackSpeed = 1f;
	public float attackRange = 10f;
	public float projectileArcHeight = 5f;

	[Header("----- ECONOMY -----")]
	public int cost = 50;
}
