using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefense/TowerData")]

public class TowerData : ScriptableObject
{
	public enum TargetPriorityMode
		{
			Closest,
			Random,
			Furthest
		}

	[Header("----- PROJECTILE -----")]
	public float projectileSpeed = 10f;
	public float projectileDamage = 20f;
	public GameObject projectilePrefab;

	[Header("----- TOWER STATS -----")]
	public float maxHealth = 100f;

	[Tooltip("Attacks per second")]
	public float attackRate = 1f;	
	public float attackRange = 10f;
	public float projectileArcHeight = 5f;
	public TargetPriorityMode targetPriorityMode = TargetPriorityMode.Closest; // How tower selects enemy target


	[Header("----- ECONOMY -----")]
	public int cost = 50;

}
