using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems; 
using TMPro;

[System.Serializable]
public class TowerPlacementOption
{
    public GameObject towerPrefab;
    public GameObject previewMesh;
    public TowerData towerData;
}

public class PlacementManager : MonoBehaviour
{
    [Header("----- TOWER OPTIONS -----")]
    public List<TowerPlacementOption> towerOptions = new List<TowerPlacementOption>();

    public Camera cam;
    [SerializeField] private CubeManager cubeManager;
    [SerializeField] private PlayerManager playerManager;

    private int selectedTowerIndex = -1; // -1 means no selection
    private GameObject previewObject;
    private SnapPoint currentSnapPoint;
    private bool isPlacing = false;
    [SerializeField] private Texture2D buildCursor;


    [Header("----- UPGRADE -----")]
    [SerializeField] private Texture2D upgradeCursor;
    [SerializeField] private TextMeshProUGUI upgradeCostTextUI;
    public bool inUpgradeMode = false;
    private Tower hoveredTowerForUpgrade;

    [Header("----- CAMERA SHAKE -----")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    void Update()
    {
        HandleTowerSelection();

        // Show build cursor when placing
        if (isPlacing)
        {
            Vector2 hotspot = new Vector2(buildCursor.width / 2f, buildCursor.height / 2f);
            Cursor.SetCursor(buildCursor, hotspot, CursorMode.Auto);
            
            UpdatePreviewPosition();
            HandlePlacement();
        }
        else if (inUpgradeMode)
        {
            UpdateUpgradePreview();
            HandleUpgradeClick();
        }
        else
        {
            // Reset cursor if not placing or upgrading
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && (isPlacing || inUpgradeMode))
        {
            CancelPlacement();
            ExitUpgradeMode();
        }
    }

    void HandleTowerSelection()
    {
        if (inUpgradeMode) return; // Ignore placement keys in upgrade mode

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

    public void EnterUpgradeMode()
    {
        inUpgradeMode = true;
        isPlacing = false;
        selectedTowerIndex = -1;
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        currentSnapPoint = null;

        Vector2 hotspot = new Vector2(upgradeCursor.width / 2f, upgradeCursor.height / 2f);
        Cursor.SetCursor(upgradeCursor, hotspot, CursorMode.Auto);

            if (upgradeCostTextUI != null)
            {
                upgradeCostTextUI.gameObject.SetActive(false);
            }

        CancelPlacement();
    }

    public void ExitUpgradeMode()
    {
        inUpgradeMode = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (upgradeCostTextUI != null)
        {
            upgradeCostTextUI.gameObject.SetActive(false);
            upgradeCostTextUI.text = "";
        }
        hoveredTowerForUpgrade = null;
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

        ExitUpgradeMode();
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
        // Check if pointer is over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Exit if clicking on UI
        }

        if (previewObject != null && currentSnapPoint != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceObject(currentSnapPoint);
        }
    }

    void PlaceObject(SnapPoint snapPoint)
    {
        if (towerOptions.Count > selectedTowerIndex && towerOptions[selectedTowerIndex].towerPrefab != null && towerOptions[selectedTowerIndex].towerData != null)
        {
            int cost = towerOptions[selectedTowerIndex].towerData.cost;
            if (playerManager != null && playerManager.SpendGold(cost))
            {
                snapPoint.PlaceTower(towerOptions[selectedTowerIndex].towerPrefab);
            }
            else
            {
                CantAfford();
            }
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

    private void UpdateUpgradePreview()
{
    hoveredTowerForUpgrade = null;
    if (upgradeCostTextUI != null)
    {
        upgradeCostTextUI.gameObject.SetActive(false);
        upgradeCostTextUI.text = "";
    }

    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        Tower tower = hit.collider.GetComponentInParent<Tower>();
        if (tower != null)
        {
            float nextCost = tower.GetNextUpgradeCost();
            if (nextCost > 0f)
            {
                hoveredTowerForUpgrade = tower;
                if (upgradeCostTextUI != null)
                {
                    upgradeCostTextUI.text = $"{nextCost:F0}G";
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    RectTransform rect = upgradeCostTextUI.GetComponent<RectTransform>();
                    rect.position = new Vector3(mousePos.x + 10f, mousePos.y - 10f, 0f);
                    upgradeCostTextUI.gameObject.SetActive(true);
                }
            }
        }
    }
}

    private void HandleUpgradeClick()
    {
        // Check if pointer is over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && hoveredTowerForUpgrade != null)
        {
            float cost = hoveredTowerForUpgrade.GetNextUpgradeCost();
            if (cost > 0f && playerManager != null && playerManager.SpendGold((int)cost))
            {
                hoveredTowerForUpgrade.Upgrade();
            }
            else
            {
                CantAfford();
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            ExitUpgradeMode();
        }
    }

    void CantAfford()
    {
        // Camera shake
        if (cam != null)
        {
            StartCoroutine(ShakeCamera(shakeDuration, shakeMagnitude));
        }
        // To do: Add flash logic
    }

    // Call this from UI buttons to set active placement
    public void SetActivePlacement(int index)
    {
        if (index >= 0 && index < towerOptions.Count)
        {
            selectedTowerIndex = index;
            isPlacing = true;
            CreatePreviewObject();
        }
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cam.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = originalPos;
    }

    private void CancelPlacement()
    {
        selectedTowerIndex = -1;
        isPlacing = false;
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        currentSnapPoint = null;
    }
}