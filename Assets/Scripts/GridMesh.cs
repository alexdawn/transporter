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
        Vector3 centre = cell.transform.localPosition;
        Vector3 vs0 = centre + GridMetrics.GetSolidEdge(GridDirection.SW);
        Vector3 vs1 = centre + GridMetrics.GetSolidEdge(GridDirection.NW);
        Vector3 vs2 = centre + GridMetrics.GetSolidEdge(GridDirection.NE);
        Vector3 vs3 = centre + GridMetrics.GetSolidEdge(GridDirection.SE);
        vs0 = vs0 + Vector3.up * (cell.vertexElevations.Y0) * GridMetrics.elevationStep;
        vs1 = vs1 + Vector3.up * (cell.vertexElevations.Y1) * GridMetrics.elevationStep;
        vs2 = vs2 + Vector3.up * (cell.vertexElevations.Y2) * GridMetrics.elevationStep;
        vs3 = vs3 + Vector3.up * (cell.vertexElevations.Y3) * GridMetrics.elevationStep;

        Vector3 vb0 = centre + GridMetrics.GetEdge(GridDirection.SW);
        Vector3 bridgeW = GridMetrics.GetBridge(GridDirection.W);
        Vector3 vb1 = centre + GridMetrics.GetEdge(GridDirection.NW);
        Vector3 bridgeN = GridMetrics.GetBridge(GridDirection.N);
        Vector3 vb2 = centre + GridMetrics.GetEdge(GridDirection.NE);
        Vector3 bridgeE = GridMetrics.GetBridge(GridDirection.E);
        Vector3 vb3 = centre + GridMetrics.GetEdge(GridDirection.SE);
        Vector3 bridgeS = GridMetrics.GetBridge(GridDirection.S);
        vb0 = vb0 + Vector3.up * (cell.vertexElevations.Y0) * GridMetrics.elevationStep;
        vb1 = vb1 + Vector3.up * (cell.vertexElevations.Y1) * GridMetrics.elevationStep;
        vb2 = vb2 + Vector3.up * (cell.vertexElevations.Y2) * GridMetrics.elevationStep;
        vb3 = vb3 + Vector3.up * (cell.vertexElevations.Y3) * GridMetrics.elevationStep;
        if (vs0.y == vs2.y)  // sets direction of the triangle pairs in the quad
        {
            AddTriangle(vs0, vs1, vs2);
            if(cell.GetNeighbor(GridDirection.N))
            {
                AddQuad(vs1, vs1 + 2 * bridgeN, vs2 + 2 * bridgeN, vs2);
            }
            else  // only draw to edge of cell
            {
                AddQuad(vs1, vs1 + bridgeN, vs2 + bridgeN, vs2);
            }
            if(cell.GetNeighbor(GridDirection.W) == null) // only draw if no neighbouring cell
            {
                AddQuad(vs1, vs0, vs0 + bridgeW, vs1 + bridgeW);
            }
            AddQuad(vs1, vs1 + bridgeW, vb1, vs1 + bridgeN); // NW Corner
            AddQuad(vs2, vs2 + bridgeN, vb2, vs2 + bridgeE); // NE Corner
            AddColors(cell, GridDirection.NW, true, cell.GetNeighbor(GridDirection.W) == null);

            AddTriangle(vs0, vs2, vs3);
            if(cell.GetNeighbor(GridDirection.S) == null)
            {
                AddQuad(vs3, vs3 + bridgeS, vs0 + bridgeS, vs0);
            }
            if (cell.GetNeighbor(GridDirection.E))
            {
                AddQuad(vs3, vs2, vs2 + 2 * bridgeE, vs3 + 2 * bridgeE);
            }
            else
            {
                AddQuad(vs3, vs2, vs2 + bridgeE, vs3 + bridgeE);
            }
            AddQuad(vs3, vs3 + bridgeE, vb3, vs3 + bridgeS); // SE Corner
            AddQuad(vs0, vs0 + bridgeS, vb0, vs0 + bridgeW); // SW Corner
            AddColors(cell, GridDirection.SE, cell.GetNeighbor(GridDirection.S) == null, true);
        }
        else
        {
            AddTriangle(vs0, vs1, vs3);
            if(cell.GetNeighbor(GridDirection.W) == null)
            {
                AddQuad(vs0, vs0 + bridgeW, vs1 + bridgeW, vs1);
            }
            if(cell.GetNeighbor(GridDirection.S) == null)
            {
                AddQuad(vs0, vs3, vs3 + bridgeS, vs0 + bridgeS);
            }
            AddQuad(vs0, vs0 + bridgeS, vb0, vs0 + bridgeW); // SW Corner
            AddQuad(vs1, vs1 + bridgeW, vb1, vs1 + bridgeN); // NW Corner
            AddColors(cell, GridDirection.SW, cell.GetNeighbor(GridDirection.W) == null, cell.GetNeighbor(GridDirection.S) == null);

            AddTriangle(vs2, vs3, vs1);
            if (cell.GetNeighbor(GridDirection.E))
            {
                AddQuad(vs2, vs2 + 2 * bridgeE, vs3 + 2 * bridgeE, vs3);
            }
            else
            {
                AddQuad(vs2, vs2 + bridgeE, vs3 + bridgeE, vs3);
            }
            if (cell.GetNeighbor(GridDirection.N))
            {
                AddQuad(vs2, vs1, vs1 + 2 * bridgeN, vs2 + 2 * bridgeN);
            }
            else
            {
                AddQuad(vs2, vs1, vs1 + bridgeN, vs2 + bridgeN);
            }
            AddQuad(vs2, vs2 + bridgeN, vb2, vs2 + bridgeE); // NE Corner
            AddQuad(vs3, vs3 + bridgeE, vb3, vs3 + bridgeS); // SE Corner
            AddColors(cell, GridDirection.NE, true, true);
        }

    }

    void AddColors(SquareCell cell, GridDirection direction, bool nextEdge, bool prevEdge)
    {
        SquareCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        SquareCell neighbor = cell.GetNeighbor(direction) ?? cell;
        SquareCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;
        SquareCell next2Neighbor = cell.GetNeighbor(direction.Next2()) ?? cell;
        SquareCell next3Neighbor = cell.GetNeighbor(direction.Next3()) ?? cell;

        Color previousMix = (prevNeighbor.color + cell.color) / 2f;
        Color neighborMix = (neighbor.color + cell.color + prevNeighbor.color + nextNeighbor.color) / 4f;
        Color nextMix = (nextNeighbor.color + cell.color) / 2f;
        Color nextNeighborMix = (nextNeighbor.color + cell.color + next2Neighbor.color + next3Neighbor.color) / 4f;
        Color next2Mix = (next3Neighbor.color + cell.color) / 2f;
        AddTriangleColor(cell.color);
        if (nextEdge) AddQuadColor(cell.color, nextNeighbor.color, nextNeighbor.color, cell.color); // next Edge
        if (prevEdge) AddQuadColor(cell.color, cell.color, prevNeighbor.color, prevNeighbor.color);  // previous Edge
        AddQuadColor(cell.color, previousMix, neighborMix, nextMix);  // direction Corner
        AddQuadColor(cell.color, nextMix, nextNeighborMix, next2Mix);  // next Corner
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


    void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }


    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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


    void AddQuadColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }


        void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
}