using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;
    public float arcHeight = 5f;

    private Vector3 startPos;
    private float journeyLength;
    private float startTime;

    void Start()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        startPos = transform.position;
        journeyLength = Vector3.Distance(startPos, target.position);
        startTime = Time.time;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        fractionOfJourney = Mathf.Clamp01(fractionOfJourney);

        // Calculate the next position along a straight line
        Vector3 nextPos = Vector3.Lerp(startPos, target.position, fractionOfJourney);
        // Add arc
        nextPos.y += arcHeight * Mathf.Sin(Mathf.PI * fractionOfJourney);
        transform.position = nextPos;

        if (fractionOfJourney >= 1f)
        {
            // Hit target
            Destroy(gameObject);
        }
    }
}
