
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


[System.Serializable]
public class TowerPlacementOption
{
    public GameObject towerPrefab;
    public GameObject previewMesh;
}

public class PlacementManager : MonoBehaviour
{
    [Header("----- TOWER OPTIONS -----")]
    public List<TowerPlacementOption> towerOptions = new List<TowerPlacementOption>();
    public Camera cam;
    [SerializeField] private CubeManager cubeManager;

    private int selectedTowerIndex = -1; // -1 means no selection
    private GameObject previewObject;
    private SnapPoint currentSnapPoint;
    private bool isPlacing = false;

    void Update()
    {
        HandleTowerSelection();
        if (isPlacing)
        {
            UpdatePreviewPosition();
            HandlePlacement();
        }
    }

    void HandleTowerSelection()
    {
        // Hardcoded key bindings 1-9
        for (int i = 0; i < Mathf.Min(9, towerOptions.Count); i++)
        {
            if (Keyboard.current[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
            {
                selectedTowerIndex = i;
                isPlacing = true;
                CreatePreviewObject();
            }
        }
    }

    void CreatePreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        if (isPlacing && towerOptions.Count > selectedTowerIndex && towerOptions[selectedTowerIndex].previewMesh != null)
        {
            previewObject = Instantiate(towerOptions[selectedTowerIndex].previewMesh);
            SetPreviewMode(previewObject, true);
        }
    }

    void SetPreviewMode(GameObject obj, bool isPreview)
    {
        // Make preview semi-transparent and disable colliders/scripts
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                Color c = mat.color;
                mat.color = new Color(c.r, c.g, c.b, isPreview ? 0.5f : 1f);
            }
        }
        foreach (var col in obj.GetComponentsInChildren<Collider>())
        {
            col.enabled = !isPreview;
        }
        foreach (var mono in obj.GetComponentsInChildren<MonoBehaviour>())
        {
            mono.enabled = !isPreview;
        }
    }

    void UpdatePreviewPosition()
    {
        if (previewObject == null) return;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("SnapPoint"))
            {
                SnapPoint snapPoint = hit.collider.GetComponent<SnapPoint>();
                if (snapPoint != null && !snapPoint.isOccupied)
                {
                    previewObject.SetActive(true);
                    previewObject.transform.position = snapPoint.transform.position;
                    previewObject.transform.rotation = snapPoint.transform.rotation;
                    currentSnapPoint = snapPoint;
                    return;
                }
            }
        }
        previewObject.SetActive(false);
        currentSnapPoint = null;
    }

    void HandlePlacement()
    {
        if (previewObject != null && currentSnapPoint != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceObject(currentSnapPoint);
        }
    }

    void PlaceObject(SnapPoint snapPoint)
    {
        if (towerOptions.Count > selectedTowerIndex && towerOptions[selectedTowerIndex].towerPrefab != null)
        {
            snapPoint.PlaceTower(towerOptions[selectedTowerIndex].towerPrefab);
        }
        // Reset selection and preview after placement
        selectedTowerIndex = -1;
        isPlacing = false;
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        currentSnapPoint = null;
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