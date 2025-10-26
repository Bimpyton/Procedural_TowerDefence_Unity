using UnityEngine;

public class CanvasSway : MonoBehaviour
{
    public float swayAmount = 5f;
    public float swaySpeed = 1f;
    public float directionChangeSpeed = 0.5f;

    private Vector3 initialPosition;
    private float currentAngle;

    void Start()
    {
        initialPosition = transform.localPosition;
        currentAngle = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        currentAngle += directionChangeSpeed * Time.deltaTime;
        Vector2 swayDirection = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));

        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        Vector3 offset = new Vector3(swayDirection.x, swayDirection.y, 0) * sway;
        transform.localPosition = initialPosition + offset;
    }
}