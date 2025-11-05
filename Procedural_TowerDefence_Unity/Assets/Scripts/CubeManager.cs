using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [Header("----- CUBE MATERIALS -----")]
    public Material[] waterMaterial;
    [SerializeField] private Material[] middleMaterial;
    [SerializeField] private Material[] highMaterial;

    [Header("----- CUBE HEIGHTS -----")]
    [SerializeField] private float waterHeight = 1f;
    [SerializeField] private float middleHeight = 8f;

    [Header("----- CUBE LIST -----")]
    [SerializeField] private List<GameObject> cubes = new List<GameObject>();

    void Start()
    {
        FindCubesInScene();
        SetMaterialsByHeight();
    }

    void FindCubesInScene()
    {
        GameObject[] foundCubes = GameObject.FindGameObjectsWithTag("Cube");
        cubes.AddRange(foundCubes);

    }

    public void SetMaterialsByHeight()
    {
        foreach (GameObject cube in cubes)
        {
            Transform childTransform = null;

            Renderer renderer = cube.GetComponentInChildren<Renderer>();
            if (renderer == null) continue;

            float cubeHeight = cube.transform.position.y;

            if (cubeHeight < waterHeight)
            {
                renderer.material = randomRange(waterMaterial);
                childTransform = cube.transform.Find("SnapPoint");
            }
            else if (cubeHeight < middleHeight)
            {
                renderer.material = randomRange(middleMaterial);
            }
            else
            {
                renderer.material = randomRange(highMaterial); 
                childTransform = cube.transform.Find("SnapPoint");
            }



            if (childTransform != null)
            {
                GameObject childGameObject = childTransform.gameObject;

                childGameObject.SetActive(false);
            }
        }
    }

    private Material randomRange(Material[] materials)
    {
        int index = UnityEngine.Random.Range(0, materials.Length);
        return materials[index];
    }
}