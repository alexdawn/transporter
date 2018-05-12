using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{

    Mesh gridMesh;
    MeshCollider meshCollider;
    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colors;


    void Awake()
    {
        GetComponent<MeshFilter>().mesh = gridMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        gridMesh.name = "Grid Mesh";
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();
    }


    public void Triangulate(SquareCell[] cells)
    {
        gridMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        gridMesh.vertices = vertices.ToArray();
        gridMesh.colors = colors.ToArray();
        gridMesh.triangles = triangles.ToArray();
        meshCollider.sharedMesh = gridMesh;
        gridMesh.RecalculateNormals();
    }


    void Triangulate(SquareCell cell)
    {
        Vector3 edge = cell.transform.localPosition - new Vector3(0.5f, 0, 0.5f);
        AddTriangle(edge, edge + new Vector3(0f, 0f, 1f), edge + new Vector3(1f, 0f));
        AddTriangleColor(cell.color);
        edge = cell.transform.localPosition + new Vector3(0.5f, 0, 0.5f);
        AddTriangle(edge, edge - new Vector3(0f, 0f, 1f), edge - new Vector3(1f, 0f));
        AddTriangleColor(cell.color);
    }


    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }
}