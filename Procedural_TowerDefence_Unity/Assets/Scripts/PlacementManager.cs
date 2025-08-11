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
                    PlaceObject(hit.collider.transform.position);
                    hit.collider.gameObject.SetActive(false);
                }
            }
        }
    }

    void PlaceObject(Vector3 snapPosition)
    {
        Instantiate(towerPrefab, snapPosition, Quaternion.identity);
    }
}
