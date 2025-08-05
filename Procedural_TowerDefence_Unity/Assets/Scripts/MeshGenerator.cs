using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateShape()
    {
        // Randomize noise offsets for each generation
        noiseOffsetX = UnityEngine.Random.Range(0f, 10000f);
        noiseOffsetZ = UnityEngine.Random.Range(0f, 10000f);

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
            if (edge == 0) { startX = UnityEngine.Random.Range(0, xSize); startZ = 0; }
            if (edge == 1) { startX = xSize; startZ = UnityEngine.Random.Range(0, zSize); }
            if (edge == 2) { startX = UnityEngine.Random.Range(0, xSize); startZ = zSize; }

            var path = new System.Collections.Generic.List<Vector2Int>();
            int x = startX, z = startZ;
            while (Mathf.Abs(x - centerX) > 1 || Mathf.Abs(z - centerZ) > 1)
            {
                path.Add(new Vector2Int(x, z));
                // Move towards center with some randomness
                if (x != centerX)
                    x += Mathf.Clamp(centerX - x + UnityEngine.Random.Range(-1, 2), -1, 1);
                if (z != centerZ)
                    z += Mathf.Clamp(centerZ - z + UnityEngine.Random.Range(-1, 2), -1, 1);
                x = Mathf.Clamp(x, 0, xSize);
                z = Mathf.Clamp(z, 0, zSize);
            }
            path.Add(new Vector2Int(centerX, centerZ));
            riverPaths.Add(path);
        }

        // Lower river path vertices and widen
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
                            // Use riverSlope variable for bank curve
                            float depth = Mathf.Lerp(riverDepth, vertices[idx].y, Mathf.Pow(dist / riverWidth, riverSlope));
                            vertices[idx].y = Mathf.Min(vertices[idx].y, depth);
                        }
                    }
                }
            }
        }

        // Lower the center for the tower
        int centerIdx = centerZ * (xSize + 1) + centerX;
        vertices[centerIdx].y = riverDepth - 2f;

        // Update min/max terrain height
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y > maxTerrainHeight)
                maxTerrainHeight = vertices[i].y;
            if (vertices[i].y < minTerrainHeight)
                minTerrainHeight = vertices[i].y;
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        colors = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
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
        return
            Mathf.PerlinNoise((x + noiseOffsetX) / noise01Scale, (z + noiseOffsetZ) / noise01Scale) * noise01Amp +
            Mathf.PerlinNoise((x + noiseOffsetX) / noise02Scale, (z + noiseOffsetZ) / noise02Scale) * noise02Amp +
            Mathf.PerlinNoise((x + noiseOffsetX) / noise03Scale, (z + noiseOffsetZ) / noise03Scale) * noise03Amp;
    }

#if UNITY_EDITOR
public void RegenerateTerrain()
{
    CreateShape();
    UpdateMesh();
}
#endif
}
