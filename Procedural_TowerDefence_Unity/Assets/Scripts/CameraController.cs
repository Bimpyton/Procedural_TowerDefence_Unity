using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("----- REFERENCES -----")]
    public Transform centerPoint;
    public Camera mainCamera;

    [Header("----- ZOOM -----")]
    public float minZoom = 10f;
    public float maxZoom = 50f;
    public float zoomSpeed = 10f;
    public float zoomSmoothTime = 0.1f;

    [Header("----- ROTATION -----")]
    public float rotationSpeed = 70f;
    public float pitchSpeed = 50f;
    public float minPitch = 20f;
    public float maxPitch = 80f;
    public float rotationSmoothTime = 0.1f;

    [Header("----- SWAY -----")]
    public float swayAmplitude = 0.5f;
    public float swayFrequency = 1f;

    private float currentZoom = 15f;
    private float currentYaw = 0f;
    private float currentPitch = 45f;
    private float targetZoom;
    private float targetYaw;
    private float targetPitch;
    private float zoomVelocity = 0f;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();
        targetZoom = currentZoom;
        targetYaw = currentYaw;
        targetPitch = currentPitch;
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
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Yaw (left/right)
        if (Keyboard.current.dKey.isPressed)
            targetYaw -= rotationSpeed * Time.deltaTime;
        if (Keyboard.current.aKey.isPressed)
            targetYaw += rotationSpeed * Time.deltaTime;

        // Pitch (up/down)
        if (Keyboard.current.wKey.isPressed)
            targetPitch -= pitchSpeed * Time.deltaTime;
        if (Keyboard.current.sKey.isPressed)
            targetPitch += pitchSpeed * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    void UpdateCamera()
    {
        if (centerPoint == null || mainCamera == null)
            return;

        // Smoothly interpolate toward target values
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime / rotationSmoothTime);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime / rotationSmoothTime);

        // Calculate base offset using spherical coordinates
        float pitchRad = Mathf.Deg2Rad * currentPitch;
        float yawRad = Mathf.Deg2Rad * currentYaw;

        Vector3 offset = new Vector3(
            currentZoom * Mathf.Sin(pitchRad) * Mathf.Sin(yawRad),
            currentZoom * Mathf.Cos(pitchRad),
            currentZoom * Mathf.Sin(pitchRad) * Mathf.Cos(yawRad)
        );

        // Add subtle sway
        float time = Time.time;
        Vector3 swayOffset = new Vector3(
            Mathf.Sin(time * swayFrequency) * swayAmplitude,
            Mathf.Cos(time * swayFrequency * 0.7f) * swayAmplitude * 0.5f,
            Mathf.Sin(time * swayFrequency * 0.8f) * swayAmplitude
        );

        // Apply position and look at center
        transform.position = centerPoint.position + offset + swayOffset;
        transform.LookAt(centerPoint.position);

        mainCamera.orthographicSize = currentZoom;
    }
}