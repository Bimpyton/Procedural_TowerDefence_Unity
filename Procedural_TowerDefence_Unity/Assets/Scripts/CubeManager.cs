using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public Material waterMaterial;
    public Material middleMaterial;
    public Material highMaterial;

    // Set your height thresholds here
    public float waterHeight = 1f;
    public float middleHeight = 8f;

    void Start()
    {
        SetMaterialByHeight();
    }

    void SetMaterialByHeight()
    {
        float y = transform.position.y;
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null) return;

        if (y < waterHeight)
        {
            renderer.material = waterMaterial;
        }
        else if (y < middleHeight)
        {
            renderer.material = middleMaterial;
        }
        else
        {
            renderer.material = highMaterial;
        }
    }
}
