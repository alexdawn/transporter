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
            AddMeshForSquare(cells[i]);
            AddCliffEdges(cells[i]);
        }
        gridMesh.vertices = vertices.ToArray();
        gridMesh.colors = colors.ToArray();
        gridMesh.triangles = triangles.ToArray();
        meshCollider.sharedMesh = gridMesh;
        gridMesh.RecalculateNormals();
    }


    Vector3 GetVertexBlendElevation(GridElevations el, GridDirection direction, GridDirection edge)
    {
        float delta = GetStraightGrad(el, edge) * GridMetrics.blendFactor / 2;
        if (edge == direction.Next()) //fix clockwise direction
        {
            delta *= -1;
        }
        return Vector3.up * GridMetrics.elevationStep * ((float)el[direction] + delta);
    }


    Vector3 GetDoubleVertexBlendElevation(GridElevations el, GridDirection direction)
    {
        float delta = (GetStraightGrad(el, direction.Previous()) - GetStraightGrad(el, direction.Next())) * GridMetrics.blendFactor / 2;
        return Vector3.up * GridMetrics.elevationStep * ((float)el[direction] + delta);
    }


    float GetStraightGrad(GridElevations el, GridDirection direction)
    {
        return (float)el[direction.Previous()] - (float)el[direction.Next()];
    }


    void AddCliffEdges(SquareCell cell)
    {
        // only need to add on north and east sides to prevent dupes
        AddCliffEdge(cell, GridDirection.N);
        AddCliffEdge(cell, GridDirection.E);
    }


    void AddCliffEdge(SquareCell cell, GridDirection direction)
    {
        GridDirection prev = direction.Previous();
        GridDirection next = direction.Next();
        GridDirection prevN = prev.Previous2();
        GridDirection nextN = next.Next2();
        SquareCell neighbor = cell.GetNeighbor(direction);
        if (neighbor != null)
        {
            Vector3 c0 = new Vector3(cell.coordinates.X, 0, cell.coordinates.Z) + GridMetrics.GetEdge(prev) + Vector3.up * (int)cell.GridElevations[prev] * GridMetrics.elevationStep;
            Vector3 c1 = new Vector3(cell.coordinates.X, 0, cell.coordinates.Z) + GridMetrics.GetEdge(next) + Vector3.up * (int)cell.GridElevations[next] * GridMetrics.elevationStep;
            Vector3 n0 = new Vector3(neighbor.coordinates.X, 0, neighbor.coordinates.Z) + GridMetrics.GetEdge(prevN) + Vector3.up * (int)neighbor.GridElevations[prevN] * GridMetrics.elevationStep;
            Vector3 n1 = new Vector3(neighbor.coordinates.X, 0, neighbor.coordinates.Z) + GridMetrics.GetEdge(nextN) + Vector3.up * (int)neighbor.GridElevations[nextN] * GridMetrics.elevationStep;
            if (c0 != n0 && c1 != n1)
            {
                //Debug.Log(string.Format("square"));
                //Debug.Log(string.Format("{0} {1}", c0, c1));
                //Debug.Log(string.Format("{0} {1}", n0, n1));
                AddQuad(c0, n0, n1, c1);
                AddQuadColor(Color.gray);
            }
            else if (c0 != n0)
            {
                //Debug.Log(string.Format("tri 0"));
                //Debug.Log(string.Format("{0} {1}", c0, c1));
                //Debug.Log(string.Format("{0} {1}", n0, n1));
                AddTriangle(c1, c0, n0);
                AddTriangleColor(Color.gray);
            }
            else if (c1 != n1)
            {
                //Debug.Log(string.Format("tri 1"));
                //Debug.Log(string.Format("{0} {1}", c0, c1));
                //Debug.Log(string.Format("{0} {1}", n0, n1));
                AddTriangle(c0, n1, c1);
                AddTriangleColor(Color.gray);
            }
        }
    }

    void AddMeshForSquare(SquareCell cell)
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

        Vector3 vS0 = vs0 + bridgeS + GetVertexBlendElevation(cell.GridElevations, GridDirection.SW, GridDirection.S);
        Vector3 vW0 = vs0 + bridgeW + GetVertexBlendElevation(cell.GridElevations, GridDirection.SW, GridDirection.W);
        Vector3 vN1 = vs1 + bridgeN + GetVertexBlendElevation(cell.GridElevations, GridDirection.NW, GridDirection.N);
        Vector3 vW1 = vs1 + bridgeW + GetVertexBlendElevation(cell.GridElevations, GridDirection.NW, GridDirection.W);
        Vector3 vN2 = vs2 + bridgeN + GetVertexBlendElevation(cell.GridElevations, GridDirection.NE, GridDirection.N);
        Vector3 vE2 = vs2 + bridgeE + GetVertexBlendElevation(cell.GridElevations, GridDirection.NE, GridDirection.E);
        Vector3 vE3 = vs3 + bridgeE + GetVertexBlendElevation(cell.GridElevations, GridDirection.SE, GridDirection.E);
        Vector3 vS3 = vs3 + bridgeS + GetVertexBlendElevation(cell.GridElevations, GridDirection.SE, GridDirection.S);

        if (cell.GridElevations.Y0 == cell.GridElevations.Y2)  // keep diagonals level
        {
            vs0 += Vector3.up * (cell.GridElevations.Y0) * GridMetrics.elevationStep;
            vs1 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.NW);
            vs2 += Vector3.up * (cell.GridElevations.Y2) * GridMetrics.elevationStep;
            vs3 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.SE);
        }
        else if (cell.GridElevations.Y1 == cell.GridElevations.Y3)
        {
            vs0 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.SW);
            vs1 += Vector3.up * (cell.GridElevations.Y1) * GridMetrics.elevationStep;
            vs2 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.NE);
            vs3 += Vector3.up * (cell.GridElevations.Y3) * GridMetrics.elevationStep;
        }
        else
        {
            vs0 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.SW);
            vs1 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.NW);
            vs2 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.NE);
            vs3 += GetDoubleVertexBlendElevation(cell.GridElevations, GridDirection.SE);
        }

        // Work out which diagonal to add
        if (cell.GridElevations.Y0 == cell.GridElevations.Y2 || Mathf.Abs(cell.GridElevations.Y0 - cell.GridElevations.Y2) < Mathf.Abs(cell.GridElevations.Y1 - cell.GridElevations.Y3))  // sets direction of the triangle pairs in the quad
        {
            AddHalfCell(cell, GridDirection.NW, centre, vs0, vs1, vs2, vN1, vN2, vW0, vW1, vb1, vb2, vE2);
            AddHalfCell(cell, GridDirection.SE, centre, vs2, vs3, vs0, vS3, vS0, vE2, vE3, vb3, vb0, vW0);
        }
        else
        {
            AddHalfCell(cell, GridDirection.SW, centre, vs3, vs0, vs1, vW0, vW1, vS3, vS0, vb0, vb1, vN1);
            AddHalfCell(cell, GridDirection.NE, centre, vs1, vs2, vs3, vE2, vE3, vN1, vN2, vb2, vb3, vS3);
        }

    }

    void AddHalfCell(SquareCell cell, GridDirection direction, Vector3 centre, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 va, Vector3 vb, Vector3 vc, Vector3 vd, Vector3 vx, Vector3 vy, Vector3 vz)
    {
        Vector3 riverbed = centre + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midSolidEdgePrev = centre + GridMetrics.GetSolidEdge(direction.Previous()) + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midSolidEdgeNext = centre + GridMetrics.GetSolidEdge(direction.Next()) + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midEdgePrev = centre + GridMetrics.GetEdge(direction.Previous()) + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midEdgeNext = centre + GridMetrics.GetEdge(direction.Next()) + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            {
                AddTriangle(v0, midSolidEdgePrev, riverbed);
                AddTriangle(midSolidEdgePrev, v1, riverbed);
            }
            else
            { AddTriangle(v0, v1, riverbed); }
            if (cell.HasRiverThroughEdge(direction.Next()))
            {
                AddTriangle(v1, midSolidEdgeNext, riverbed);
                AddTriangle(midSolidEdgeNext, v2, riverbed);
            }
            else
            { AddTriangle(v1, v2, riverbed); }
        }
        else
            { AddTriangle(v0, v1, v2); }
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            AddQuad(v1, va, midEdgeNext, midSolidEdgeNext);
            AddQuad(midSolidEdgeNext, midEdgeNext, vb, v2);
        }
        else
            { AddQuad(v1, va, vb, v2); } // top Edge
        if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            AddQuad(v1, midSolidEdgePrev, midEdgePrev, vd);
            AddQuad(midSolidEdgePrev, v0, vc, midEdgePrev);
        }
        else
            { AddQuad(v1, v0, vc, vd); }  // left Edge
        if (cell.HasRiverThroughEdge(direction))
        {
        }
        else
            { AddQuad(v1, vd, vx, va); } // direction edge 
        if (cell.HasRiverThroughEdge(direction.Next2()))
        {
        }
        else
            { AddQuad(v2, vb, vy, vz); }// clockwise edge
        AddColors(cell, direction);
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
        if(cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            { AddTriangleColor(cell.Color); }
            if (cell.HasRiverThroughEdge(direction.Next()))
            { AddTriangleColor(cell.Color); }
            AddTriangleColor(cell.Color);
            AddTriangleColor(cell.Color);
        }
        else
        {
            AddTriangleColor(cell.Color);
        }
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            AddQuadColor(cell.Color, nextMix, nextMix, cell.Color);
            AddQuadColor(cell.Color, nextMix, nextMix, cell.Color);
        }
        else
            { AddQuadColor(cell.Color, nextMix, nextMix, cell.Color); } // next Edge
        if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            AddQuadColor(cell.Color, cell.Color, previousMix, previousMix);
            AddQuadColor(cell.Color, cell.Color, previousMix, previousMix);
        }
        else
            { AddQuadColor(cell.Color, cell.Color, previousMix, previousMix); }  // previous Edge
        if (cell.HasRiverThroughEdge(direction))
            { }
        else
            { AddQuadColor(cell.Color, previousMix, neighborMix, nextMix); } // direction Corner
        if (cell.HasRiverThroughEdge(direction.Next2()))
            { }
        else
            { AddQuadColor(cell.Color, nextMix, nextNeighborMix, next2Mix); }// next Corner
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