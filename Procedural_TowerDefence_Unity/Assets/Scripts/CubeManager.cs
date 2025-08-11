using System;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [Header("----- CUBE MATERIALS -----")]
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material middleMaterial;
    [SerializeField] private Material highMaterial;

    [Header("----- CUBE HEIGHTs -----")]
    private float cubeHeight;
    [SerializeField] private float waterHeight = 1f;
    [SerializeField] private float middleHeight = 8f;

    [Header("----- CHILDREN -----")]
    [SerializeField] private GameObject snapPoint;

    void Start()
    {
        SetMaterialByHeight();
    }

    void SetMaterialByHeight()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null) return;

        cubeHeight = transform.position.y;

        if (cubeHeight < waterHeight)
        {
            renderer.material = waterMaterial;
        }
        else if (cubeHeight < middleHeight)
        {
            renderer.material = middleMaterial;
        }
        else
        {
            renderer.material = highMaterial;
        }
    }
}
