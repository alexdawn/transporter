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
        Vector3 v0 = GridMetrics.GetBottomLeftVertex(cell.transform.localPosition);
        Vector3 v1 = GridMetrics.GetTopLeftVertex(cell.transform.localPosition);
        Vector3 v2 = GridMetrics.GetTopRightVertex(cell.transform.localPosition);
        Vector3 v3 = GridMetrics.GetBottomRightVertex(cell.transform.localPosition);
        v0 = v0 + Vector3.up * (cell.centreElevation + cell.vertexElevations.Y0) * GridMetrics.elevationStep;
        v1 = v1 + Vector3.up * (cell.centreElevation + cell.vertexElevations.Y1) * GridMetrics.elevationStep;
        v2 = v2 + Vector3.up * (cell.centreElevation + cell.vertexElevations.Y2) * GridMetrics.elevationStep;
        v3 = v3 + Vector3.up * (cell.centreElevation + cell.vertexElevations.Y3) * GridMetrics.elevationStep;
        AddTriangle(v0, v1, v3);
        AddTriangleColor(cell.color);
        AddTriangle(v2, v3, v1);
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