using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{

    Mesh gridMesh;
    MeshCollider meshCollider;
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Color> colors = new List<Color>();


    void Awake()
    {
        GetComponent<MeshFilter>().mesh = gridMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        gridMesh.name = "Grid Mesh";
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


    Vector3 GetSolidElevation(GridElevations el, GridDirection direction)
    {
        float oppositeHeight = (float)el[direction.Opposite()];
        float fractionalHeight = GridMetrics.solidFactor * ((float)el[direction] - oppositeHeight);
        return Vector3.up * (fractionalHeight + oppositeHeight) * GridMetrics.elevationStep;
    }


    Vector3 GetBridgeElevation(GridElevations el, GridDirection vertexDirection, GridDirection bridgeDirection)
    {
        float delta = ((float)el[bridgeDirection.Previous()] - (float)el[bridgeDirection.Next()]) * GridMetrics.blendFactor;
        return Vector3.up * ((float)el[vertexDirection] - delta) * GridMetrics.elevationStep;
    }


    void Triangulate(SquareCell cell)
    {
        Vector3 centre = cell.transform.localPosition;

        Vector3 vb0 = centre + GridMetrics.GetEdge(GridDirection.SW);
        Vector3 vb1 = centre + GridMetrics.GetEdge(GridDirection.NW);
        Vector3 vb2 = centre + GridMetrics.GetEdge(GridDirection.NE);
        Vector3 vb3 = centre + GridMetrics.GetEdge(GridDirection.SE);
        vb0 = vb0 + Vector3.up * (cell.GridElevations.Y0) * GridMetrics.elevationStep;
        vb1 = vb1 + Vector3.up * (cell.GridElevations.Y1) * GridMetrics.elevationStep;
        vb2 = vb2 + Vector3.up * (cell.GridElevations.Y2) * GridMetrics.elevationStep;
        vb3 = vb3 + Vector3.up * (cell.GridElevations.Y3) * GridMetrics.elevationStep;

        Vector3 vs0 = centre + GridMetrics.GetSolidEdge(GridDirection.SW);
        Vector3 vs1 = centre + GridMetrics.GetSolidEdge(GridDirection.NW);
        Vector3 vs2 = centre + GridMetrics.GetSolidEdge(GridDirection.NE);
        Vector3 vs3 = centre + GridMetrics.GetSolidEdge(GridDirection.SE);


        Vector3 bridgeW = GridMetrics.GetBridge(GridDirection.W);
        Vector3 bridgeN = GridMetrics.GetBridge(GridDirection.N);
        Vector3 bridgeE = GridMetrics.GetBridge(GridDirection.E);
        Vector3 bridgeS = GridMetrics.GetBridge(GridDirection.S);

        Vector3 vS0 = vs0 + bridgeS + GetBridgeElevation(cell.GridElevations, GridDirection.SW, GridDirection.S);
        Vector3 vW0 = vs0 + bridgeW + GetBridgeElevation(cell.GridElevations, GridDirection.SW, GridDirection.W);
        Vector3 vN1 = vs1 + bridgeN + GetBridgeElevation(cell.GridElevations, GridDirection.NW, GridDirection.N);
        Vector3 vW1 = vs1 + bridgeW + GetBridgeElevation(cell.GridElevations, GridDirection.NW, GridDirection.W);
        Vector3 vN2 = vs2 + bridgeN + GetBridgeElevation(cell.GridElevations, GridDirection.NE, GridDirection.N);
        Vector3 vE2 = vs2 + bridgeE + GetBridgeElevation(cell.GridElevations, GridDirection.NE, GridDirection.E);
        Vector3 vE3 = vs3 + bridgeE + GetBridgeElevation(cell.GridElevations, GridDirection.SE, GridDirection.E);
        Vector3 vS3 = vs3 + bridgeS + GetBridgeElevation(cell.GridElevations, GridDirection.SE, GridDirection.S);

        vs0 = vs0 + GetSolidElevation(cell.GridElevations, GridDirection.SW);
        vs1 = vs1 + GetSolidElevation(cell.GridElevations, GridDirection.NW);
        vs2 = vs2 + GetSolidElevation(cell.GridElevations, GridDirection.NE);
        vs3 = vs3 + GetSolidElevation(cell.GridElevations, GridDirection.SE);
        if (vs0.y == vs2.y)  // sets direction of the triangle pairs in the quad
        {
            AddTriangle(vs0, vs1, vs2);
            AddQuad(vs1, vN1, vN2, vs2);
            AddQuad(vs1, vs0, vW0, vW1);
            AddQuad(vs1, vW1, vb1, vN1); // NW Corner
            AddQuad(vs2, vN2, vb2, vE2); // NE Corner
            AddColors(cell, GridDirection.NW);

            AddTriangle(vs0, vs2, vs3);
            AddQuad(vs3, vS3, vS0, vs0);
            AddQuad(vs3, vs2, vE2, vE3);
            AddQuad(vs3, vE3, vb3, vS3); // SE Corner
            AddQuad(vs0, vS0, vb0, vW0); // SW Corner
            AddColors(cell, GridDirection.SE);
        }
        else
        {
            AddTriangle(vs0, vs1, vs3);
            AddQuad(vs0, vW0, vW1, vs1);
            AddQuad(vs0, vs3, vS3, vS0);
            AddQuad(vs0, vS0, vb0, vW0); // SW Corner
            AddQuad(vs1, vW1, vb1, vN1); // NW Corner
            AddColors(cell, GridDirection.SW);

            AddTriangle(vs2, vs3, vs1);
            AddQuad(vs2, vE2, vE3, vs3);
            AddQuad(vs2, vs1, vN1, vN2);
            AddQuad(vs2, vN2, vb2, vE2); // NE Corner
            AddQuad(vs3, vE3, vb3, vS3); // SE Corner
            AddColors(cell, GridDirection.NE);
        }

    }

    void AddColors(SquareCell cell, GridDirection direction)
    {
        SquareCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        SquareCell neighbor = cell.GetNeighbor(direction) ?? cell;
        SquareCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;
        SquareCell next2Neighbor = cell.GetNeighbor(direction.Next2()) ?? cell;
        SquareCell next3Neighbor = cell.GetNeighbor(direction.Next3()) ?? cell;

        Color previousMix = (prevNeighbor.Color + cell.Color) / 2f;
        Color neighborMix = (neighbor.Color + cell.Color + prevNeighbor.Color + nextNeighbor.Color) / 4f;
        Color nextMix = (nextNeighbor.Color + cell.Color) / 2f;
        Color nextNeighborMix = (nextNeighbor.Color + cell.Color + next2Neighbor.Color + next3Neighbor.Color) / 4f;
        Color next2Mix = (next3Neighbor.Color + cell.Color) / 2f;
        AddTriangleColor(cell.Color);
        AddQuadColor(cell.Color, nextNeighbor.Color, nextNeighbor.Color, cell.Color); // next Edge
        AddQuadColor(cell.Color, cell.Color, prevNeighbor.Color, prevNeighbor.Color);  // previous Edge
        AddQuadColor(cell.Color, previousMix, neighborMix, nextMix);  // direction Corner
        AddQuadColor(cell.Color, nextMix, nextNeighborMix, next2Mix);  // next Corner
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