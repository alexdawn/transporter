using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SquareCell : MonoBehaviour {
    public SquareGridChunk parentChunk;
    public GridCoordinates coordinates;
    public TownManager Town;
    public Transform ExplosionPrefab;

    float centreElevation = 0;
    GridElevations vertexElevations;
    GroundMaterial tile;
    Building buildingOnSquare;
    int waterLevel = 2;
    int urbanLevel = 0;
    int plantLevel = 0;
    int farmLevel = 0;
    int scenaryObject = 0;
    int industry = 0;

    [SerializeField]
    SquareCell[] neighbors;

    [SerializeField]
    private bool[] hasIncomingRivers = new bool[8];
    [SerializeField]
    private bool[] hasOutgoingRivers = new bool[8];
    [SerializeField]
    bool[] roads = new bool[8]; // includes diagonals


    public void Demolish()
    {
        urbanLevel = 0;
        plantLevel = 0;
        farmLevel = 0;
        scenaryObject = 0;
        industry = 0;
        if(buildingOnSquare != null)
        {
            buildingOnSquare.Demolish();
        }
        GroundMaterial.SetToMud(this, ref tile);
        Explosion();
    }

    public void Explosion()
    {
        Transform explosion = Instantiate(ExplosionPrefab);
        explosion.position = GridCoordinates.ToPosition(coordinates) + new Vector3(0, CentreElevation * GridMetrics.elevationStep, 0);
        Destroy(explosion.gameObject, 10);
    }

    public Building BuildingOnSquare
    {
        set
        {
            buildingOnSquare = value;
            plantLevel = 0;
            Refresh();
        }
        get
        {
            return buildingOnSquare;
        }
    }

    public int Industry
    {
        get
        {
            return industry;
        }
        set
        {
            industry = value;
            plantLevel = 0;
            urbanLevel = 0;
            farmLevel = 0;
            scenaryObject = 0;
            Refresh();
        }
    }

    public int ScenaryObject
    {
        get 
        {
            return scenaryObject;
        }
        set
        {
            scenaryObject = Mathf.Clamp(value, 0, 3);
            plantLevel = 0;
            urbanLevel = 0;
            farmLevel = 0;
            if(value == 0)
            {
                if(buildingOnSquare != null)
                {
                    buildingOnSquare.Demolish();
                }
            }
            scenaryObject = value;
            Refresh();
        }
    }

    public int UrbanLevel
    {
        get
        {
            return urbanLevel;
        }
        set
        {
            if (urbanLevel != value)
            {
                Flatten();
                urbanLevel = Mathf.Clamp(value, 0, 3);
                plantLevel = 0;
                scenaryObject = 0;
                farmLevel = 0;
                GroundMaterial.SetToMud(this, ref tile);
                Refresh();
            }
        }
    }

    public int FarmLevel
    {
        get
        {
            return farmLevel;
        }
        set
        {
            if (farmLevel != value)
            {
                farmLevel = Mathf.Clamp(value, 0, 1);
                plantLevel = 0;
                urbanLevel = 0;
                Refresh();
            }
        }
    }

    public int PlantLevel
    {
        get
        {
            return plantLevel;
        }
        set
        {
            if (plantLevel != value)
            {   
                if (value == 0)
                {
                    GroundMaterial.SetToMud(this, ref tile);
                }
                plantLevel = Mathf.Clamp(value, 0, 6);
                urbanLevel = 0;
                farmLevel = 0;
                scenaryObject = 0;
                Refresh();
            }
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            Refresh();
        }
    }

    public bool IsFullyUnderwater
    {
        get
        {
            return waterLevel > GetMaxElevation();
        }
    }

    public bool IsPartUnderwater
    {
        get
        {
            if (!IsFullyUnderwater)
            {
                return IsUnderwater;
            }
            return false;
        }
    }

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > GetMinElevation();
        }
    }


    public bool HasRoadThroughEdge(GridDirection direction)
    {
        return roads[(int)direction];
    }


    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    public int RoadCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    counter++;
                }
            }
            return counter;
        }
    }

    public void AddRoad(GridDirection direction)
    {
        if (!roads[(int)direction] && !HasRiver && !HasCliff(direction) && GetElevationDifference(direction) <= 3)
        {
            SetRoad((int)direction, true);
        }
        else
        {
            Debug.Log("Could not add road");
        }
    }

    public void RemoveRoad(GridDirection direction)
    {
        if (roads[(int)direction] == true)
        {
            SetRoad((int)direction, false);
        }
    }

    void SetRoad(int direction, bool state)
    {
        roads[direction] = state;
        GroundMaterial.SetToMud(this, ref tile);
        Refresh();
    }

    public void Flatten()
    {
        if (GetMaxElevation() != GetMinElevation())
        {
            FlattenTo(GetMaxElevation());
        }
    }

    public void FlattenTo(int height)
    {
        vertexElevations.Y0 = height;
        vertexElevations.Y1 = height;
        vertexElevations.Y2 = height;
        vertexElevations.Y3 = height;
        UpdateCentreElevation();
    }

    public int GetElevationDifference(GridDirection direction)
    {
        int differencePrev = (int)vertexElevations[direction.Previous()] - (int)vertexElevations[direction.Opposite().Next()];
        int differenceNext = (int)vertexElevations[direction.Next()] - (int)vertexElevations[direction.Opposite().Previous()];
        int result = Mathf.Max(differencePrev, differenceNext);
        return result;
    }

    public int GetMaxElevation()
    {
        return Mathf.Max(vertexElevations.Y0, vertexElevations.Y1, vertexElevations.Y2, vertexElevations.Y3);
    }

    public int GetMinElevation()
    {
        return Mathf.Min(vertexElevations.Y0, vertexElevations.Y1, vertexElevations.Y2, vertexElevations.Y3);
    }

    public GridElevations GetVertexElevations
    {
        get{
            return vertexElevations;
        }
    }

    public bool HasCliff(GridDirection direction)
    {
        SquareCell neighbor = GetNeighbor(direction);
        if (neighbor)
        {
            bool noCliff = (int)vertexElevations[direction.Previous()] == (int)neighbor.vertexElevations[direction.Opposite().Next()] && (int)vertexElevations[direction.Next()] == (int)neighbor.vertexElevations[direction.Opposite().Previous()];
            return !noCliff;
        }
        else
        {
            return false;
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return
                (centreElevation + GridMetrics.waterElevationOffset) * GridMetrics.elevationStep;
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return
                (waterLevel + GridMetrics.waterElevationOffset) * GridMetrics.elevationStep;
        }
    }

    public bool[] HasIncomingRiver
    {
        get
        {
            return hasIncomingRivers;
        }
    }

    public bool[] HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRivers;
        }
    }

    public GridDirection[] IncomingRivers
    {
        get
        {
            List<GridDirection> directions = new List<GridDirection>();
            for (int i = 0; i < 8; i++)
            {
                if (hasIncomingRivers[i])
                {
                    directions.Add((GridDirection)i);
                }
            }
            return directions.ToArray();
        }
    }

    public GridDirection[] OutgoingRivers
    {
        get
        {
            List<GridDirection> directions = new List<GridDirection>();
            for (int i = 0; i < 8; i++)
            {
                if (hasOutgoingRivers[i])
                {
                    directions.Add((GridDirection)i);
                }
            }
            return directions.ToArray();
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRivers.Any(item => item == true) || hasOutgoingRivers.Any(item => item == true);
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRivers.Any(item => item == true) != hasOutgoingRivers.Any(item => item == true);
        }
    }

    public bool HasRiverThroughEdge(GridDirection direction)
    {
        return hasIncomingRivers[(int)direction] || hasOutgoingRivers[(int)direction];
    }

    public void RemoveOutgoingRivers()
    {
        if (!hasOutgoingRivers.Any())
        {
            return;
        }
        GridDirection[] neighbors = OutgoingRivers;
        for (int i = 0; i < hasOutgoingRivers.Length; i++) { hasOutgoingRivers[i] = false; }
        GroundMaterial.SetToMud(this, ref tile);
        RefreshChunkOnly();

        foreach (GridDirection direction in neighbors)
        {
            SquareCell neighbor = GetNeighbor(direction);
            hasIncomingRivers[(int)direction.Opposite()] = false;
            neighbor.RefreshChunkOnly();
        }
    }

    public void RemoveOutgoingRiver(GridDirection direction)
    {
        if (!hasOutgoingRivers[(int)direction])
        {
            return;
        }
        hasOutgoingRivers[(int)direction] = false;
        RefreshChunkOnly();
        SquareCell neighbor = GetNeighbor(direction);
        hasIncomingRivers[(int)direction.Opposite()] = false;
        neighbor.RefreshChunkOnly();
    }

    public void RemoveIncomingRivers()
    {
        if (!hasIncomingRivers.Any())
        {
            return;
        }
        GridDirection[] neighbors = IncomingRivers;
        for (int i = 0; i < hasIncomingRivers.Length; i++) { hasIncomingRivers[i] = false; }
        GroundMaterial.SetToMud(this, ref tile);
        RefreshChunkOnly();

        foreach (GridDirection direction in neighbors)
        {
            SquareCell neighbor = GetNeighbor(direction);
            hasOutgoingRivers[(int)direction.Opposite()] = false;
            neighbor.RefreshChunkOnly();
        }
    }

    public void RemoveIncomingRiver(GridDirection direction)
    {
        if (!hasIncomingRivers[(int)direction])
        {
            return;
        }
        hasIncomingRivers[(int)direction] = false;
        RefreshChunkOnly();
        SquareCell neighbor = GetNeighbor(direction);
        hasOutgoingRivers[(int)direction.Opposite()] = false;
        neighbor.RefreshChunkOnly();
    }

    public void RemoveRivers()
    {
        RemoveOutgoingRivers();
        RemoveIncomingRivers();
    }


    public void SetOutgoingRiver(GridDirection direction)
    {
        if (hasOutgoingRivers[(int)direction] || HasRoads)
        {
            Debug.Log("Could not add river");
            return;
        }
        SquareCell neighbor = GetNeighbor(direction);
        if (!neighbor || centreElevation < neighbor.centreElevation)
        {
            Debug.Log("Could not add river uphill");
            return;
        }
        if (hasIncomingRivers[(int)direction])
        {
            RemoveIncomingRiver(direction);
        }
        hasOutgoingRivers[(int)direction] = true;
        RefreshChunkOnly();

        neighbor.RemoveOutgoingRiver(direction.Opposite());
        neighbor.hasIncomingRivers[(int)direction.Opposite()] = true;
        neighbor.RefreshChunkOnly();
    }


    public void SetIncomingRiver(GridDirection direction)
    {
        if (hasOutgoingRivers[(int)direction] || HasRoads)
        {
            Debug.Log("Could not add river");
            return;
        }
        SquareCell neighbor = GetNeighbor(direction);
        if (!neighbor || centreElevation < neighbor.centreElevation)
        {
            Debug.Log("Could not add river uphill");
            return;
        }
        if (hasIncomingRivers[(int)direction])
        {
            RemoveIncomingRiver(direction.Opposite());
        }
    }

    private void UpdateCentreElevation()
    {
        centreElevation = (vertexElevations.Y0 + vertexElevations.Y1 + vertexElevations.Y2 + vertexElevations.Y3) / 4f;
        int maxElevation = Mathf.Max(vertexElevations.Y0, vertexElevations.Y1, vertexElevations.Y2, vertexElevations.Y3);
        for (GridDirection i = GridDirection.N; i < GridDirection.NW; i++)
        {
            if (hasOutgoingRivers[(int)i] && centreElevation < GetNeighbor(i).centreElevation)
            {
                RemoveOutgoingRiver(i);
            }
            if (hasIncomingRivers[(int)i] && centreElevation > GetNeighbor(i).centreElevation)
            {
                RemoveIncomingRiver(i);
            }
        }
        Refresh();
    }

    public float CentreElevation
    {
        get { return centreElevation; }
    }


    public void ChangeVertexElevation(GridDirection vertex, int value)
    {
        if (value == 0 && ((HasRoadThroughEdge(vertex.Next()) && (GetElevationDifference(vertex.Next()) > 3 || HasCliff(vertex.Next()))) ||
            (HasRoadThroughEdge(vertex.Previous()) && (GetElevationDifference(vertex.Previous()) > 3 || HasCliff(vertex.Previous())))
            ))
        {
            Debug.Log("Could not change elevation");
        }
        else
        {
            vertexElevations[vertex] += value;
            GroundMaterial.SetToMud(this, ref tile);
            UpdateCentreElevation();
        }
    }

    // shouldn't be public in addition to above?
    public GridElevations GridElevations
    {
        get { return vertexElevations; }
        set
        {
            vertexElevations = value;
            UpdateCentreElevation();
        }
    }

    public GroundMaterial Tile
    {
        get { return tile; }
        set {
            tile = value;
            Refresh();
        }
    }


    public Color Color
    {
        get { return tile.color; }
    }

    public bool BlendEdge
    {
        get { return tile.blendEdge; }
    }


    public SquareCell GetNeighbor(GridDirection direction)
    {
        return neighbors[(int)direction];
    }


    public void SetNeighbor(GridDirection direction, SquareCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }


    public void Refresh()
    {
        if (parentChunk)
        {
            parentChunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                SquareCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.parentChunk != parentChunk)
                {
                    neighbor.parentChunk.Refresh();
                }
            }
        }
    }

    void RefreshChunkOnly()
    {
        parentChunk.Refresh();
    }
}
