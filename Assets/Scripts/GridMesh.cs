using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{
    public bool useCollider, useColors, useUVCoordinates;
    Mesh gridMesh;
    MeshCollider meshCollider;
    [NonSerialized] List<Vector3> vertices;
    [NonSerialized] List<int> triangles;
    [NonSerialized] List<Color> colors;
    [NonSerialized] List<Vector2> uvs;


    void Awake()
    {
        GetComponent<MeshFilter>().mesh = gridMesh = new Mesh();
        if (useCollider)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        gridMesh.name = "Grid Mesh";
    }

    public void Clear()
    {
        gridMesh.Clear();
        vertices = ListPool<Vector3>.Get();
        if (useColors)
        {
            colors = ListPool<Color>.Get();
        }
        if (useUVCoordinates)
        {
            uvs = ListPool<Vector2>.Get();
        }
        triangles = ListPool<int>.Get();
    }

    public void Apply()
    {
        gridMesh.SetVertices(vertices);
        ListPool<Vector3>.Add(vertices);
        if (useColors)
        {
            gridMesh.SetColors(colors);
            ListPool<Color>.Add(colors);
        }
        if (useUVCoordinates)
        {
            gridMesh.SetUVs(0, uvs);
            ListPool<Vector2>.Add(uvs);
        }
        gridMesh.SetTriangles(triangles, 0);
        ListPool<int>.Add(triangles);
        gridMesh.RecalculateNormals();
        if (useCollider)
        {
            meshCollider.sharedMesh = gridMesh;
        }
    }

    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    public void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector3 uv3)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }

    public void AddTriangleUV(float uMin, float uMax, float vMin, float vMax)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
    }

    public void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }


    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }


    public void AddQuadColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }

    public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMax, vMax));
        uvs.Add(new Vector2(uMin, vMax));
    }

    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
}