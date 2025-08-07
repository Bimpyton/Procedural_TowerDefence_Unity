using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("----- REFERENCES -----")]
    public Transform centerPoint;

    [Header("----- ZOOM -----")]
    public float minZoom = 10f;
    public float maxZoom = 50f;
    public float zoomSpeed = 10f;

    [Header("----- ROTATION -----")]
    public float rotationSpeed = 70f;
    public float pitchSpeed = 50f;
    public float minPitch = 20f;
    public float maxPitch = 80f;

    private float currentZoom = 30f;
    private float currentYaw = 0f;
    private float currentPitch = 45f;

    void Start()
    {
        
    }

    void Update()
    {
        HandleInput();
        UpdateCamera();
    }

    void HandleInput()
    {
        // Zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        currentZoom -= scroll * zoomSpeed * Time.deltaTime;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Yaw (left/right)
        if (Keyboard.current.dKey.isPressed)
            currentYaw -= rotationSpeed * Time.deltaTime;
        if (Keyboard.current.aKey.isPressed)
            currentYaw += rotationSpeed * Time.deltaTime;

        // Pitch (up/down)
        if (Keyboard.current.wKey.isPressed)
            currentPitch -= pitchSpeed * Time.deltaTime;
        if (Keyboard.current.sKey.isPressed)
            currentPitch += pitchSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    void UpdateCamera()
    {
        if (centerPoint == null)
            return;

        // Calculate offset using spherical coordinates
        float pitchRad = Mathf.Deg2Rad * currentPitch;
        float yawRad = Mathf.Deg2Rad * currentYaw;

        Vector3 offset = new Vector3(
            currentZoom * Mathf.Sin(pitchRad) * Mathf.Sin(yawRad),
            currentZoom * Mathf.Cos(pitchRad),
            currentZoom * Mathf.Sin(pitchRad) * Mathf.Cos(yawRad)
        );

        transform.position = centerPoint.position + offset;
        transform.LookAt(centerPoint.position);
    }
}
