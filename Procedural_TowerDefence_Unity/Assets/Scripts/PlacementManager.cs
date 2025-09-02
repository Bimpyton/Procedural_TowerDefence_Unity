using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    public GameObject towerPrefab; // Prefab you want to place
    public Camera cam;
    [SerializeField] private CubeManager cubeManager;

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
                    SnapPoint snapPoint = hit.collider.GetComponent<SnapPoint>();
                    if (snapPoint != null && !snapPoint.isOccupied)
                    {
                        PlaceObject(snapPoint);
                    }
                }
            }
        }
    }

    void PlaceObject(SnapPoint snapPoint)
    {
        snapPoint.PlaceTower(towerPrefab);
        if (snapPoint.isOccupied && snapPoint.transform.childCount > 0)
        {
            Transform towerTransform = snapPoint.transform.GetChild(snapPoint.transform.childCount - 1);
        }
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
                float value = 1f - Mathf.Exp(-springiness * progress) * Mathf.Cos(progress * Mathf.PI * springiness);

            target.localScale = Vector3.LerpUnclamped(startScale, finalScale, value);
            yield return null;
        }

        target.localScale = finalScale; // Snap to final
    }
}