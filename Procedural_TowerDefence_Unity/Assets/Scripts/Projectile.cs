using UnityEngine;


public class Projectile : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;
    public float arcHeight = 5f;
    public int damage = 20;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float journeyLength;
    private float startTime;
    private bool targetLost = false;

    void Start()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        startPos = transform.position;
        targetPos = target.position;
        journeyLength = Vector3.Distance(startPos, targetPos);
        startTime = Time.time;
    }

    void Update()
    {
        if (!targetLost && target == null)
        {
            // Target lost, keep moving toward last known position
            targetLost = true;
        }
        if (!targetLost)
        {
            targetPos = target.position;
        }
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        fractionOfJourney = Mathf.Clamp01(fractionOfJourney);

        // Calculate the next position along a straight line
        Vector3 nextPos = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
        // Add arc
        nextPos.y += arcHeight * Mathf.Sin(Mathf.PI * fractionOfJourney);
        transform.position = nextPos;

        if (fractionOfJourney >= 1f)
        {
            // Hit target or land at last known position
            if (!targetLost && target != null)
            {
                if (target.CompareTag("Enemy"))
                {
                    Enemy enemy = target.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                }
                else if (target.CompareTag("Tower"))
                {
                    Tower tower = target.GetComponent<Tower>();
                    if (tower != null)
                    {
                        tower.TakeDamage(damage);
                    }
                }
            }
            // Optionally play a landing effect here
            Destroy(gameObject);
        }
    }
}
