using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareCell : MonoBehaviour {
    public GridCoordinates coordinates;
    public Color color;
    public RectTransform uiRect;

    int centreElevation = 0;
    GridElevations vertexElevations;

    [SerializeField]
    SquareCell[] neighbors;


    private void UpdateCentreElevation()
    {
        centreElevation = (vertexElevations.Y0 + vertexElevations.Y1 + vertexElevations.Y2 + vertexElevations.Y3) / 4;
        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = centreElevation * -GridMetrics.elevationStep;
        uiRect.localPosition = uiPosition;
        Debug.Log(string.Format("Update Elevation {0}", centreElevation));
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


    public SquareCell GetNeighbor(GridDirection direction)
    {
        return neighbors[(int)direction];
    }


    public void SetNeighbor(GridDirection direction, SquareCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
}
