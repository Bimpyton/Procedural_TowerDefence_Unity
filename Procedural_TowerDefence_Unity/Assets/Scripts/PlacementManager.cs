using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    public GameObject towerPrefab; // Prefab you want to place
    public Camera cam;
    [SerializeField] private CubeManager cubeManager;
    [SerializeField] private float springiness = 4f; // Adjust for more or less spring effect
    [SerializeField] private float springTime = 1f;
    [SerializeField] private float startScale = 0.01f;

    void Update()
    {
        // Check if the left mouse button was pressed down this frame
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("SnapPoint"))
                {
                    PlaceObject(hit.collider.transform.position);
                    hit.collider.gameObject.SetActive(false);
                }
            }
        }
    }

    void PlaceObject(Vector3 snapPosition)
    {
        GameObject tower = Instantiate(towerPrefab, snapPosition, Quaternion.identity);

        // Start tiny so the spring effect can animate it
        tower.transform.localScale = Vector3.one * startScale;

        StartCoroutine(SpringScale(tower.transform, Vector3.one, springTime, springiness));
}


    private System.Collections.IEnumerator SpringScale(Transform target, Vector3 finalScale, float duration, float springiness)
    {
        float time = 0f;
        Vector3 startScale = target.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            // Spring formula
            float value = Mathf.Sin(progress * Mathf.PI * springiness) * (1f - progress) + progress;

            target.localScale = Vector3.LerpUnclamped(startScale, finalScale, value);
            yield return null;
        }

        target.localScale = finalScale; // Snap to final

    }
}