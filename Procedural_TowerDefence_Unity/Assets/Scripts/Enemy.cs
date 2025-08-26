using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public int health = 10;
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private float waypointThreshold = 0.5f;

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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainTower"))
        {
            Debug.Log($"Enemy hit Main Tower");
            Destroy(gameObject);
        }
        else
        {
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterEnemy();
        }
    }
}