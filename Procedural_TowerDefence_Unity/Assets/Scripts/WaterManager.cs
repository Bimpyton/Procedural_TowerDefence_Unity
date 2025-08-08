using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterManager : MonoBehaviour
{
    public int xSize = 20;
    public int zSize = 20;
    public float noiseAmp = 2f;
    public float noiseScale = 0.3f;

    private Mesh mesh;
    private Vector3[] vertices;
    private float[,] noiseOffsets;

    void Start()
    {
        // Generate random offsets for each vertex
        noiseOffsets = new float[xSize + 1, zSize + 1];
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                noiseOffsets[x, z] = Random.Range(0f, 1000f);
            }
        }
        GenerateMesh();
    }

    void Update()
    {
        UpdateMesh();
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * noiseAmp;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        int[] triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void UpdateMesh()
    {
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float offset = noiseOffsets[x, z];
                float y = Mathf.PerlinNoise(
                    x * noiseScale + Time.time + offset,
                    z * noiseScale + Time.time + offset
                ) * noiseAmp;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
