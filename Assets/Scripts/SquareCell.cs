using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SquareCell : MonoBehaviour {
    public SquareGridChunk parentChunk;
    public GridCoordinates coordinates;
    public RectTransform uiRect;

    int centreElevation = 0;
    GridElevations vertexElevations;
    Color color;

    [SerializeField]
    SquareCell[] neighbors;

    private bool[] hasIncomingRivers = new bool[8];
    private bool[] hasOutgoingRivers = new bool[8];


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
            for (int i = 0; i <= 8; i++)
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
            for (int i = 0; i <= 8; i++)
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
        if ((int)direction >= hasIncomingRivers.Length)
        { Debug.Log((int)direction); }
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
        RefreshSelfOnly();

        foreach (GridDirection direction in neighbors)
        {
            SquareCell neighbor = GetNeighbor(direction);
            hasIncomingRivers[(int)direction.Opposite()] = false;
            neighbor.RefreshSelfOnly();
        }
    }

    public void RemoveOutgoingRiver(GridDirection direction)
    {
        if (!hasOutgoingRivers[(int)direction])
        {
            return;
        }
        hasOutgoingRivers[(int)direction] = false;
        RefreshSelfOnly();
        SquareCell neighbor = GetNeighbor(direction);
        hasIncomingRivers[(int)direction.Opposite()] = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRivers()
    {
        if (!hasIncomingRivers.Any())
        {
            return;
        }
        GridDirection[] neighbors = IncomingRivers;
        for (int i = 0; i < hasIncomingRivers.Length; i++) { hasIncomingRivers[i] = false; }
        RefreshSelfOnly();

        foreach (GridDirection direction in neighbors)
        {
            SquareCell neighbor = GetNeighbor(direction);
            hasOutgoingRivers[(int)direction.Opposite()] = false;
            neighbor.RefreshSelfOnly();
        }
    }

    public void RemoveIncomingRiver(GridDirection direction)
    {
        if (!hasIncomingRivers[(int)direction])
        {
            return;
        }
        hasIncomingRivers[(int)direction] = false;
        RefreshSelfOnly();
        SquareCell neighbor = GetNeighbor(direction);
        hasOutgoingRivers[(int)direction.Opposite()] = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRivers()
    {
        RemoveOutgoingRivers();
        RemoveIncomingRivers();
    }


    public void SetOutgoingRiver(GridDirection direction)
    {
        if (hasOutgoingRivers[(int)direction])
        {
            return;
        }
        SquareCell neighbor = GetNeighbor(direction);
        if (!neighbor || centreElevation < neighbor.centreElevation)
        {
            return;
        }
        if (hasIncomingRivers[(int)direction])
        {
            RemoveIncomingRiver(direction);
        }
        hasOutgoingRivers[(int)direction] = true;
        RefreshSelfOnly();

        neighbor.RemoveOutgoingRiver(direction.Opposite());
        neighbor.hasIncomingRivers[(int)direction.Opposite()] = true;
        neighbor.RefreshSelfOnly();
    }


    public void SetIncomingRiver(GridDirection direction)
    {
        if (hasOutgoingRivers[(int)direction])
        {
            return;
        }
        SquareCell neighbor = GetNeighbor(direction);
        if (!neighbor || centreElevation < neighbor.centreElevation)
        {
            return;
        }
        if (hasIncomingRivers[(int)direction])
        {
            RemoveIncomingRiver(direction.Opposite());
        }
    }

    private void UpdateCentreElevation()
    {
        centreElevation = (vertexElevations.Y0 + vertexElevations.Y1 + vertexElevations.Y2 + vertexElevations.Y3) / 4;
        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = centreElevation * -GridMetrics.elevationStep;
        uiRect.localPosition = uiPosition;
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

    public int CentreElevation
    {
        get { return centreElevation; }
    }


    public void ChangeVertexElevation(GridDirection vertex, int value)
    {
        vertexElevations[vertex] += value;
        UpdateCentreElevation();
    }


    public GridElevations GridElevations
    {
        get { return vertexElevations; }
        set
        {
            vertexElevations = value;
            UpdateCentreElevation();
        }
    }


    public Color Color
    {
        get { return color; }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
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


    void Refresh()
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

    void RefreshSelfOnly()
    {
        parentChunk.Refresh();
    }
}
