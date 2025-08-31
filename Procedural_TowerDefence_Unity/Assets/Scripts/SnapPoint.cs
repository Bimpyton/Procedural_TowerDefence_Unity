using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentTower;

    [Header("Visuals")]
    public Material hoverMaterial;
    [Range(0f, 1f)] public float hoverOpacity = 0.5f;
    private Material originalMaterial;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && hoverMaterial != null)
        {
            rend.material = hoverMaterial;
            rend.enabled = true;
            SetInvisible();
        }
    }

    void OnMouseEnter()
    {
        if (rend != null && hoverMaterial != null && !isOccupied)
        {
            Color c = rend.material.color;
            c.a = hoverOpacity;
            rend.material.color = c;
        }
    }

    void OnMouseExit()
    {
        if (rend != null && hoverMaterial != null)
        {
            SetInvisible();
        }
    }

    private void SetInvisible()
    {
        // Make the snap point invisible by setting alpha to 0
        if (rend != null && hoverMaterial != null)
        {
            Color c = rend.material.color;
            c.a = 0f;
            rend.material.color = c;
        }
    }

    public void PlaceTower(GameObject towerPrefab)
    {
        if (isOccupied) return;
        currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);
        isOccupied = true;
        var towerScript = currentTower.GetComponent<Tower>();
        if (towerScript != null)
        {
            towerScript.snapPoint = this;
        }
        SetInvisible();
    }

    public void TowerDestroyed()
    {
    isOccupied = false;
    currentTower = null;
    SetInvisible();
    }
}
