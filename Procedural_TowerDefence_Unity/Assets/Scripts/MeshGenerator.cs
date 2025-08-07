using System.Collections;
using System.Collections.Generic; // Add this at the top
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;

    public CameraController cameraController;

    [Header("----- TERRAIN SETTINGS -----")]
    public int xSize = 20;
    public int zSize = 20;

    // Multi-layer Perlin noise variables
    public float noise01Scale = 20f;
    public float noise01Amp = 18f;

    public float noise02Scale = 60f;
    public float noise02Amp = -4f;

    public float noise03Scale = 120f;
    public float noise03Amp = 2f;

    [Header("----- TEXTURE SETTINGS -----")]
    public int textureWidth = 1024;
    public int textureHeight = 1024;

    public Gradient gradient;
    private float minTerrainHeight;
    private float maxTerrainHeight;

    [Header("----- RIVER SETTINGS -----")]
    public int riverWidth = 16; // Editable in Inspector
    public float riverSlope = 2f; // Controls how steep the river banks are
    public float riverDepth = -8f; // Editable in Inspector

    private float noiseOffsetX;
    private float noiseOffsetZ;

    private List<GameObject> spawnedCubes = new List<GameObject>(); // Add this line

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        SpawnCubesAtVertices();
        UpdateMesh();

        if (cameraController != null)
        {
            // Calculate the center in world space
            Vector3 center = transform.TransformPoint(new Vector3(xSize / 2f, 0, zSize / 2f));
            GameObject centerObj = new GameObject("CameraCenterPoint");
            centerObj.transform.position = center;
            cameraController.centerPoint = centerObj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateShape()
    {
        // Randomize noise offsets for each generation
        noiseOffsetX = Random.Range(0f, 10000f);
        noiseOffsetZ = Random.Range(0f, 10000f);

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        minTerrainHeight = float.MaxValue;
        maxTerrainHeight = float.MinValue;

        // Center position
        int centerX = xSize / 2;
        int centerZ = zSize / 2;

        // Generate base terrain
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = GetNoiseSample(x, z);
                vertices[i] = new Vector3(x, y, z);
                // Track min/max
                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;
                i++;
            }
        }

        // Plot river paths
        int riverCount = 3;
        System.Collections.Generic.List<System.Collections.Generic.List<Vector2Int>> riverPaths = new();

        for (int r = 0; r < riverCount; r++)
        {
            // Pick random edge start
            int edge = r;
            int startX = 0, startZ = 0;
            if (edge == 0) { startX = Random.Range(0, xSize); startZ = 0; }
            if (edge == 1) { startX = xSize; startZ = Random.Range(0, zSize); }
            if (edge == 2) { startX = Random.Range(0, xSize); startZ = zSize; }

            var path = new System.Collections.Generic.List<Vector2Int>();
            int x = startX, z = startZ;
            while (Mathf.Abs(x - centerX) > 1 || Mathf.Abs(z - centerZ) > 1)
            {
                path.Add(new Vector2Int(x, z));
                // Move towards center with some randomness
                if (x != centerX)
                    x += Mathf.Clamp(centerX - x + Random.Range(-1, 2), -1, 1);
                if (z != centerZ)
                    z += Mathf.Clamp(centerZ - z + Random.Range(-1, 2), -1, 1);
                x = Mathf.Clamp(x, 0, xSize);
                z = Mathf.Clamp(z, 0, zSize);
            }
            path.Add(new Vector2Int(centerX, centerZ));
            riverPaths.Add(path);
        }

        // Lower river path vertices and widen
        float riverHeight = minTerrainHeight + riverDepth;
        bool[] isRiverVertex = new bool[vertices.Length];
        foreach (var path in riverPaths)
        {
            foreach (var pos in path)
            {
                for (int dz = -riverWidth; dz <= riverWidth; dz++)
                {
                    for (int dx = -riverWidth; dx <= riverWidth; dx++)
                    {
                        int nx = pos.x + dx;
                        int nz = pos.y + dz;
                        if (nx >= 0 && nx <= xSize && nz >= 0 && nz <= zSize)
                        {
                            int idx = nz * (xSize + 1) + nx;
                            float dist = Mathf.Sqrt(dx * dx + dz * dz);
                            if (dist <= riverWidth && !isRiverVertex[idx])
                            {
                                vertices[idx].y = riverHeight;
                                isRiverVertex[idx] = true;
                            }
                        }
                    }
                }
            }
        }

        // Lower the center for the tower
        int centerIdx = centerZ * (xSize + 1) + centerX;
        vertices[centerIdx].y = riverHeight - 2f;

        // Create triangles
        triangles = new int[xSize * zSize * 6];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int i = x + z * (xSize + 1);
                int ti = (x + z * xSize) * 6;
                triangles[ti] = i;
                triangles[ti + 1] = i + xSize + 1;
                triangles[ti + 2] = i + 1;
                triangles[ti + 3] = i + 1;
                triangles[ti + 4] = i + xSize + 1;
                triangles[ti + 5] = i + xSize + 2;
            }
        }

        // Create colors based on height
        colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = gradient.Evaluate(Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y));
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    float GetNoiseSample(int x, int z)
    {
        // Multi-layered Perlin noise
        float noise = 0f;
        noise += Mathf.PerlinNoise((x + noiseOffsetX) / noise01Scale, (z + noiseOffsetZ) / noise01Scale) * noise01Amp;
        noise += Mathf.PerlinNoise((x + noiseOffsetX) / noise02Scale, (z + noiseOffsetZ) / noise02Scale) * noise02Amp;
        noise += Mathf.PerlinNoise((x + noiseOffsetX) / noise03Scale, (z + noiseOffsetZ) / noise03Scale) * noise03Amp;
        return noise;
    }

    public void RegenerateTerrain()
    {
        CreateShape();
        SpawnCubesAtVertices();
        UpdateMesh();
    }

    public GameObject cubePrefab; // Assign a Cube prefab in the Inspector

    public void SpawnCubesAtVertices()
    {
        // Destroy previously spawned cubes
        foreach (var cube in spawnedCubes)
        {
            if (cube != null)
                Destroy(cube);
        }
        spawnedCubes.Clear();

        if (cubePrefab == null)
        {
            Debug.LogWarning("cubePrefab not assigned!");
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            GameObject cube = Instantiate(cubePrefab, worldPos, Quaternion.identity, transform);
            spawnedCubes.Add(cube);
        }
    }
}

