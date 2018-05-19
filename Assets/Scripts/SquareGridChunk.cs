using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareGridChunk : MonoBehaviour {
    SquareCell[] cells;

    public GridMesh terrain;
    Canvas gridCanvas;


    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        cells = new SquareCell[GridMetrics.chunkSizeX * GridMetrics.chunkSizeZ];
    }


    public void AddCell(int index, SquareCell cell)
    {
        cells[index] = cell;
        cell.parentChunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }

    public void Refresh()
    {
        enabled = true;
    }


    private void LateUpdate()
    {
        Triangulate(cells);
        enabled = false;
    }



    public void Triangulate(SquareCell[] cells)
    {
        terrain.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            AddMeshForSquare(cells[i]);
            AddCliffEdges(cells[i]);
        }
        terrain.Apply();
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
                terrain.AddQuad(c0, n0, n1, c1);
                terrain.AddQuadColor(Color.gray);
            }
            else if (c0 != n0)
            {
                //Debug.Log(string.Format("tri 0"));
                //Debug.Log(string.Format("{0} {1}", c0, c1));
                //Debug.Log(string.Format("{0} {1}", n0, n1));
                terrain.AddTriangle(c1, c0, n0);
                terrain.AddTriangleColor(Color.gray);
            }
            else if (c1 != n1)
            {
                //Debug.Log(string.Format("tri 1"));
                //Debug.Log(string.Format("{0} {1}", c0, c1));
                //Debug.Log(string.Format("{0} {1}", n0, n1));
                terrain.AddTriangle(c0, n1, c1);
                terrain.AddTriangleColor(Color.gray);
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
                terrain.AddTriangle(v0, midSolidEdgePrev, riverbed);
                terrain.AddTriangle(midSolidEdgePrev, v1, riverbed);
            }
            else
            { terrain.AddTriangle(v0, v1, riverbed); }
            if (cell.HasRiverThroughEdge(direction.Next()))
            {
                terrain.AddTriangle(v1, midSolidEdgeNext, riverbed);
                terrain.AddTriangle(midSolidEdgeNext, v2, riverbed);
            }
            else
            { terrain.AddTriangle(v1, v2, riverbed); }
        }
        else
        { terrain.AddTriangle(v0, v1, v2); }
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            terrain.AddQuad(v1, va, midEdgeNext, midSolidEdgeNext);
            terrain.AddQuad(midSolidEdgeNext, midEdgeNext, vb, v2);
        }
        else
        { terrain.AddQuad(v1, va, vb, v2); } // top Edge
        if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            terrain.AddQuad(v1, midSolidEdgePrev, midEdgePrev, vd);
            terrain.AddQuad(midSolidEdgePrev, v0, vc, midEdgePrev);
        }
        else
        { terrain.AddQuad(v1, v0, vc, vd); }  // left Edge
        if (cell.HasRiverThroughEdge(direction))
        {
        }
        else
        { terrain.AddQuad(v1, vd, vx, va); } // direction edge 
        if (cell.HasRiverThroughEdge(direction.Next2()))
        {
        }
        else
        { terrain.AddQuad(v2, vb, vy, vz); }// clockwise edge
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
        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            { terrain.AddTriangleColor(cell.Color); }
            if (cell.HasRiverThroughEdge(direction.Next()))
            { terrain.AddTriangleColor(cell.Color); }
            terrain.AddTriangleColor(cell.Color);
            terrain.AddTriangleColor(cell.Color);
        }
        else
        {
            terrain.AddTriangleColor(cell.Color);
        }
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            terrain.AddQuadColor(cell.Color, nextMix, nextMix, cell.Color);
            terrain.AddQuadColor(cell.Color, nextMix, nextMix, cell.Color);
        }
        else
        { terrain.AddQuadColor(cell.Color, nextMix, nextMix, cell.Color); } // next Edge
        if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            terrain.AddQuadColor(cell.Color, cell.Color, previousMix, previousMix);
            terrain.AddQuadColor(cell.Color, cell.Color, previousMix, previousMix);
        }
        else
        { terrain.AddQuadColor(cell.Color, cell.Color, previousMix, previousMix); }  // previous Edge
        if (cell.HasRiverThroughEdge(direction))
        { }
        else
        { terrain.AddQuadColor(cell.Color, previousMix, neighborMix, nextMix); } // direction Corner
        if (cell.HasRiverThroughEdge(direction.Next2()))
        { }
        else
        { terrain.AddQuadColor(cell.Color, nextMix, nextNeighborMix, next2Mix); }// next Corner
    }


}
