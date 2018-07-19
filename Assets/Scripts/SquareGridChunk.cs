using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareGridChunk : MonoBehaviour {
    SquareCell[] cells;
    List<SquareCell> refreshList;
    bool flagChunkRefresh = true;

    public GridMesh terrain, rivers, roads, water, waterShore;
    public GridFeatureManager features;
    public Color retainingWallColor;
    Canvas gridCanvas;


    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        cells = new SquareCell[GridMetrics.chunkSizeX * GridMetrics.chunkSizeZ];
        refreshList = new List<SquareCell>();
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
        flagChunkRefresh = true;
        enabled = true;
    }

    // doesnt currently work due to the way triangulate clears all
    public void RefreshSingle(SquareCell cell)
    {
        refreshList.Add(cell);
        enabled = true;
    }


    private void LateUpdate()
    {
        if (flagChunkRefresh)
        {
            Triangulate(cells);
            flagChunkRefresh = false;
        }
        else
        {
            Triangulate(refreshList.ToArray());
        }
        enabled = false;
    }



    public void Triangulate(SquareCell[] cells)
    {
        terrain.Clear();
        rivers.Clear();
        roads.Clear();
        water.Clear();
        waterShore.Clear();
        features.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            AddMeshForSquare(cells[i]);
            AddCliffEdges(cells[i]);
        }
        terrain.Apply();
        rivers.Apply();
        roads.Apply();
        water.Apply();
        waterShore.Apply();
        features.Apply();
    }

    Vector3 GetMidVector(Vector3 v0, Vector3 v1)
    {
        return v0 + (v1 - v0) / 2f;
    }

    void TriangulateRoadCentre(SquareCell cell, Vector3 centre, Vector3 vs0, Vector3 vs1, Vector3 vs2, Vector3 vs3)
    {
        roads.AddTriangle(centre, vs0, GetMidVector(vs0, vs1));
        roads.AddTriangle(centre, GetMidVector(vs0, vs1), vs1);
        roads.AddTriangle(centre, vs1, GetMidVector(vs1, vs2));
        roads.AddTriangle(centre, GetMidVector(vs1, vs2), vs2);
        roads.AddTriangle(centre, vs2, GetMidVector(vs2, vs3));
        roads.AddTriangle(centre, GetMidVector(vs2, vs3), vs3);
        roads.AddTriangle(centre, vs3, GetMidVector(vs3, vs0));
        roads.AddTriangle(centre, GetMidVector(vs3, vs0), vs0);

        // W
        float midU = cell.HasRoadThroughEdge(GridDirection.W) ? 1f : 0f;
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(midU, 1f));
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(midU, 1f), new Vector2(0f, 1f));
        // N
        midU = cell.HasRoadThroughEdge(GridDirection.N) ? 1f : 0f;
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(midU, 1f));
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(midU, 1f), new Vector2(0f, 1f));
        // E
        midU = cell.HasRoadThroughEdge(GridDirection.E) ? 1f : 0f;
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(midU, 1f));
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(midU, 1f), new Vector2(0f, 1f));
        // S
        midU = cell.HasRoadThroughEdge(GridDirection.S) ? 1f : 0f;
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(midU, 1f));
        roads.AddTriangleUV(new Vector2(1f, 1f), new Vector2(midU, 1f), new Vector2(0f, 1f));
    }

    void TriangulateRoadSegment(
        Vector3 v1, Vector3 v2, Vector3 v3,
        Vector3 v4, Vector3 v5, Vector3 v6
    )
    {
        roads.AddQuad(v4, v5, v2, v1);
        roads.AddQuad(v3, v2, v5, v6);
        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(0f, 1f, 0f, 0f);
    }

    void TriangulateRiverTri(Vector3 v1, Vector3 v2, Vector3 v3, float y, bool reversed, GridDirection direction, GridDirection flowDirection)
    {
        v1.y = v2.y = v3.y = y;
        rivers.AddTriangle(v1, v2, v3);
        Vector2 centre = new Vector2(0.5f, 0.5f);
        Vector2 edgePrevious, edgeNext;
        if (direction == flowDirection || direction == flowDirection.Opposite())
        {
            edgePrevious = new Vector2(0.9f, 0.9f);
            edgeNext = new Vector2(0.1f, 0.9f);
        } else
        {
            if (flowDirection == direction.Next2())
            {
                edgePrevious = new Vector2(0.9f, 0.1f);
                edgeNext = new Vector2(0.9f, 0.9f);
            }
            else
            {
                edgePrevious = new Vector2(0.1f, 0.9f);
                edgeNext = new Vector2(0.1f, 0.1f);
            }
        }
        if (reversed)
        {
            rivers.AddTriangleUV(centre, new Vector2(1f, 1f) - edgePrevious, new Vector2(1f, 1f) - edgeNext);
        }
        else
        {
            rivers.AddTriangleUV(centre, edgePrevious, edgeNext);
        }
    }

    void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float yNeighbors, float v, bool reversed)
    {
        v1.y = v2.y = y;
        v3.y = v4.y = (y + yNeighbors) / 2f;
        rivers.AddQuad(v1, v2, v3, v4);
        if (reversed)
        {
            rivers.AddQuadUV(0.9f, 0.1f, 0.1f, 0f);
        }
        else
        {
            rivers.AddQuadUV(0.1f, 0.9f, 0.9f, 1f);
        }
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
        AddCliffEdge(cell, GridDirection.N);
        AddCliffEdge(cell, GridDirection.E);
        AddCliffEdge(cell, GridDirection.S);
        AddCliffEdge(cell, GridDirection.W);
    }

    Vector2 CrossingPoint(float a, float b, float c, float d)
    {
        // where a and b are gradients and c and d are intercepts
        //returns coordinate of where lines intersect
        return new Vector2((d - c) / (a - b), a * ((d - c) / (a - b)) + c);
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
                if (c0.y > n0.y && n1.y > c1.y) // if edges cross heights along edge X
                {
                    Vector2 cross = CrossingPoint(c1.y - c0.y, n1.y - n0.y, c0.y, n0.y);
                    Debug.Log(cross);
                    Vector3 worldCross = new Vector3(cell.coordinates.X, 0, cell.coordinates.Z)
                        + GridMetrics.GetEdge(direction.Previous()) 
                        + (cross.x * 2 * GridMetrics.GetEdge(direction.Next2())) 
                        + new Vector3(0, cross.y, 0);
                    terrain.AddTriangle(worldCross, c0, n0);
                    terrain.AddTriangleColor(Color.cyan);
                    terrain.AddTriangle(worldCross, n1, c1);
                    terrain.AddTriangleColor(Color.green);
                }
                else if (c0.y > n0.y && c1.y > n1.y) // if one edge is always above the other
                {
                    terrain.AddQuad(c0, n0, n1, c1);
                    terrain.AddQuadColor(retainingWallColor);
                }
            }
            else if (c0.y > n0.y)
            {
                terrain.AddTriangle(c0, n0, c1);
                terrain.AddTriangleColor(retainingWallColor);
            }
            else if (c1.y > n1.y)
            {
                terrain.AddTriangle(c0, n1, c1);
                terrain.AddTriangleColor(retainingWallColor);
            }
        }
    }

    void AddMeshForSquare(SquareCell cell)
    {
        Vector3 centre = cell.transform.localPosition;
        Vector3 verticleCentre = centre + Vector3.up * cell.CentreElevation * GridMetrics.elevationStep;
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

        // Works out which diagonal to add
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
        if (cell.HasRoads)
        {
            TriangulateRoadCentre(cell, verticleCentre, vs0, vs1, vs2, vs3);
        }
        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(GridDirection.N))
            { TriangulateWithRiver(GridDirection.N, cell, centre, vs1, vb1, vb2, vs2, 0f); }
            else
            { TriangulateWithRiver(GridDirection.N, cell, centre, vs1, vb1, vb2, vs2, 0f); }
            if (cell.HasRiverThroughEdge(GridDirection.E))
            { TriangulateWithRiver(GridDirection.E, cell, centre, vs2, vb2, vb3, vs3, 0.5f); }
            else
            { TriangulateWithRiver(GridDirection.E, cell, centre, vs2, vb2, vb3, vs3, 0.5f); }
            if (cell.HasRiverThroughEdge(GridDirection.S))
            { TriangulateWithRiver(GridDirection.S, cell, centre, vs3, vb3, vb0, vs0, 1f); }
            else
            { TriangulateWithRiver(GridDirection.S, cell, centre, vs3, vb3, vb0, vs0, 1f); }
            if (cell.HasRiverThroughEdge(GridDirection.W))
            { TriangulateWithRiver(GridDirection.W, cell, centre, vs0, vb0, vb1, vs1, 0.5f); }
            else
            { TriangulateWithRiver(GridDirection.W, cell, centre, vs0, vb0, vb1, vs1, 0.5f); }
        }
        if(cell.IsUnderwater)
        {
            AddWaterForSquare(cell, centre);
        }
        if (!cell.IsUnderwater && !cell.HasRoads)
        {
            features.AddFeature(cell, verticleCentre);
        }
    }

    void AddWaterForSquare(SquareCell cell, Vector3 centre)
    {
        centre.y = cell.WaterSurfaceY;
        Vector3 c0 = centre + GridMetrics.GetEdge(GridDirection.SW);
        Vector3 c1 = centre + GridMetrics.GetEdge(GridDirection.NW);
        Vector3 c2 = centre + GridMetrics.GetEdge(GridDirection.NE);
        Vector3 c3 = centre + GridMetrics.GetEdge(GridDirection.SE);
        if (cell.IsFullyUnderwater)
        {
            water.AddQuad(c0, c1, c2, c3);
        }
        else if (cell.IsPartUnderwater)
        {
            float v0=0f, v1=0f, v2=0f, v3=0f;
            if (cell.GetNeighbor(GridDirection.N) && !cell.GetNeighbor(GridDirection.N).IsUnderwater) { v1 = 1f; v2 = 1f; }
            if (cell.GetNeighbor(GridDirection.NE) && !cell.GetNeighbor(GridDirection.NE).IsUnderwater) { v2 = 1f; }
            if (cell.GetNeighbor(GridDirection.E) && !cell.GetNeighbor(GridDirection.E).IsUnderwater) { v2 = 1f; v3 = 1f; }
            if (cell.GetNeighbor(GridDirection.SE) && !cell.GetNeighbor(GridDirection.SE).IsUnderwater) { v3 = 1f; }
            if (cell.GetNeighbor(GridDirection.S) && !cell.GetNeighbor(GridDirection.S).IsUnderwater) { v3 = 1f; v0 = 1f; }
            if (cell.GetNeighbor(GridDirection.SW) && !cell.GetNeighbor(GridDirection.SW).IsUnderwater) { v0 = 1f; }
            if (cell.GetNeighbor(GridDirection.W) && !cell.GetNeighbor(GridDirection.W).IsUnderwater) { v0 = 1f; v1 = 1f; }
            if (cell.GetNeighbor(GridDirection.NW) && !cell.GetNeighbor(GridDirection.NW).IsUnderwater) { v1 = 1f; }
            waterShore.AddQuad(c0, c1, c2, c3);
            waterShore.AddQuadV(v0, v1, v2, v3);
        }
    }
    
    void MakeBridge(GridMesh mesh, Vector3 v0, Vector3 v1, GridDirection bridgeDirection)
    {
        Vector3 bridge = GridMetrics.GetBridge(bridgeDirection);
        Vector3 e0 = v0 + bridge;
        Vector3 e1 = v1 + bridge;
        mesh.AddQuad(v0, e0, e1, v1);
        if (mesh.useUVCoordinates)
        {
            mesh.AddQuadUV(0f, 0f, 0f, 1f);
        }
    }

    void MakeCornerBridge(GridMesh mesh, Vector3 v0, GridDirection bridgeDirection)
    {
        Vector3 b0 = GridMetrics.GetBridge(bridgeDirection.Previous());
        Vector3 b1 = GridMetrics.GetBridge(bridgeDirection.Next());
        mesh.AddQuad(v0, v0 + b0, v0 + b0 + b1, v0 + b1);
        if (mesh.useUVCoordinates)
        {
            mesh.AddQuadUV(0f, 0f, 0f, 1f);
        }
    }

    void AddHalfCell(SquareCell cell, GridDirection direction, Vector3 centre, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 va, Vector3 vb, Vector3 vc, Vector3 vd, Vector3 vx, Vector3 vy, Vector3 vz)
    {
        Vector3 riverbed = centre + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midSolidEdgePrev = centre + GridMetrics.GetSolidEdge(direction.Previous()) + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midSolidEdgeNext = centre + GridMetrics.GetSolidEdge(direction.Next()) + Vector3.up * (cell.CentreElevation + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        SquareCell neighborPrev = cell.GetNeighbor(direction.Previous()) ?? cell;
        SquareCell neighborNext = cell.GetNeighbor(direction.Next()) ?? cell;
        Vector3 midEdgePrev = centre + GridMetrics.GetEdge(direction.Previous()) + Vector3.up * ((cell.CentreElevation + neighborPrev.CentreElevation) /2 + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        Vector3 midEdgeNext = centre + GridMetrics.GetEdge(direction.Next()) + Vector3.up * ((cell.CentreElevation + neighborNext.CentreElevation) / 2 + GridMetrics.streamBedElevationOffset) * GridMetrics.elevationStep;
        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            {
                terrain.AddTriangle(v0, midSolidEdgePrev, riverbed); // split tri into two tris
                terrain.AddTriangle(midSolidEdgePrev, v1, riverbed);
            }
            else
            { terrain.AddTriangle(v0, v1, riverbed); } // edge with no river
            if (cell.HasRiverThroughEdge(direction.Next()))
            {
                terrain.AddTriangle(v1, midSolidEdgeNext, riverbed); // split tri into two tris
                terrain.AddTriangle(midSolidEdgeNext, v2, riverbed);
            }
            else
            { terrain.AddTriangle(v1, v2, riverbed); } // edge with no river
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

        if(cell.HasRoadThroughEdge(direction.Next()))
        {
            midEdgeNext = centre + GridMetrics.GetEdge(direction.Next()) + Vector3.up * ((int)cell.GridElevations[direction] + (int)cell.GridElevations[direction.Next2()]) / 2 * GridMetrics.elevationStep;
            midSolidEdgeNext = centre + GridMetrics.GetSolidEdge(direction.Next()) + Vector3.up * ((int)cell.GridElevations[direction] + (int)cell.GridElevations[direction.Next2()]) / 2 * GridMetrics.elevationStep;
            TriangulateRoadSegment(v1, midSolidEdgeNext, v2, va, midEdgeNext, vb);
        }
        if (cell.HasRoadThroughEdge(direction.Previous()))
        {
            midEdgePrev = centre + GridMetrics.GetEdge(direction.Previous()) + Vector3.up * ((int)cell.GridElevations[direction] + (int)cell.GridElevations[direction.Previous2()]) / 2 * GridMetrics.elevationStep;
            midSolidEdgePrev = centre + GridMetrics.GetSolidEdge(direction.Previous()) + Vector3.up * ((int)cell.GridElevations[direction] + (int)cell.GridElevations[direction.Previous2()]) / 2 * GridMetrics.elevationStep;
            TriangulateRoadSegment(v0, midSolidEdgePrev, v1, vc, midEdgePrev, vd);
        }
    }

    void TriangulateWithRiver(
    GridDirection direction, SquareCell cell, Vector3 centre, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float v
)
    {
        GridDirection flowDirection;
        bool reversed = cell.HasIncomingRiver[(int)direction];
        if(cell.HasOutgoingRiver[(int)direction])
        {
            flowDirection = direction;
        }
        else if (reversed)
        {
            flowDirection = direction.Opposite();
        }
        else
        {
            if(cell.HasIncomingRiver[(int)direction.Next2()] && cell.HasOutgoingRiver[(int)direction.Previous2()])
            { flowDirection = direction.Previous2(); }
            else if (cell.HasOutgoingRiver[(int)direction.Next2()] && cell.HasIncomingRiver[(int)direction.Previous2()])
            { flowDirection = direction.Next2(); }
            else
            { flowDirection = direction.Opposite(); }
        }
        TriangulateRiverTri(centre, v0, v3, cell.RiverSurfaceY, reversed, direction, flowDirection);
        SquareCell neighbor = cell.GetNeighbor(direction);
        if (neighbor && cell.HasRiverThroughEdge(direction))
            { TriangulateRiverQuad(v3, v0, v1, v2, cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f, reversed); }
    }

    Color GetColor(SquareCell neighbour, SquareCell self)
    {
        return neighbour.BlendEdge && self.BlendEdge ? neighbour.Color : self.Color;
    }

    void AddColors(SquareCell cell, GridDirection direction)
    {
        SquareCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        SquareCell neighbor = cell.GetNeighbor(direction) ?? cell;
        SquareCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;
        SquareCell next2Neighbor = cell.GetNeighbor(direction.Next2()) ?? cell;
        SquareCell next3Neighbor = cell.GetNeighbor(direction.Next3()) ?? cell;

        Color previousMix = (GetColor(prevNeighbor, cell) + cell.Color) / 2f;
        Color neighborMix = (GetColor(neighbor, cell) + cell.Color + GetColor(prevNeighbor, cell) + GetColor(nextNeighbor, cell)) / 4f;
        Color nextMix = (GetColor(nextNeighbor, cell) + cell.Color) / 2f;
        Color nextNeighborMix = (GetColor(nextNeighbor, cell) + cell.Color + GetColor(next2Neighbor, cell) + GetColor(next3Neighbor, cell)) / 4f;
        Color next2Mix = (GetColor(next3Neighbor, cell) + cell.Color) / 2f;
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
