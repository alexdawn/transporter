using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour {
    public int chunkSize;
    public float max_height;
    public float perlin_scale;

    private int[,] grid;
    private Vector3[] vertices;
    private Mesh mesh;

    private void Awake()
    {
        Generate_Grid();
        Generate_Mesh();
    }

    private void Generate_Grid()
    {
        grid = new int[chunkSize + 1, chunkSize + 1];
        int z;
        for (float i = 0, y = 0; y <= chunkSize; y++)
        {
            for (float x = 0; x <= chunkSize; x++, i++)
            {
                z = (int)(max_height * Mathf.PerlinNoise((x / (float)chunkSize) * perlin_scale, (y / (float)chunkSize) * perlin_scale));
                grid[(int)x, (int)y] = z;
            }
        }
    }

    private void Generate_Mesh()
    {
        float z;
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        for (int i = 0, y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++, i++)
            {
                z = grid[x, y];
                vertices[i] = new Vector3(x, z, y);
            }
        }
        mesh.vertices = vertices;
        int[] triangles = new int[chunkSize * chunkSize * 6];
        for (int ti = 0, vi = 0, y = 0; y < chunkSize; y++, vi++)
        {
            for (int x = 0; x < chunkSize; x++, ti += 6, vi++)
            {
                if (grid[x, y] != grid[x + 1, y + 1])
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + chunkSize + 1;
                    triangles[ti + 5] = vi + chunkSize + 2;
                }
                else
                {
                    triangles[ti + 3] = triangles[ti] = vi;
                    triangles[ti + 1] = vi + chunkSize + 1;
                    triangles[ti + 4] = triangles[ti + 2] = vi + chunkSize + 2;
                    triangles[ti + 5] = vi + 1;
                }
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        Vector3 meshOrigin = gameObject.transform.position;
        Gizmos.color = Color.black;
        if (vertices != null) {
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(meshOrigin + vertices[i], 0.1f);
            }
        }
    }
}