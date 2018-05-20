using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EditMode
{
    color,
    elevation,
    rivers
}

public class MapEditor : MonoBehaviour {
    public Color[] colors;
    public SquareGrid squareGrid;
    private Color activeColor;
    private EditMode activeMode;
    private bool allowCliffs = false;
    private Vector3 pointerLocation;
    public int pointerSize = 1;
    bool isDrag, freshClick = false;
    GridDirection dragDirection;
    SquareCell currentCell, previousCell;
    Stopwatch stopWatch = new Stopwatch();

    void Awake()
    {
        SelectColor(0);
        stopWatch.Start();
    }


    void Update()
    {
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            freshClick = true;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            freshClick = false;
        }
        HandleMousePosition();
    }


    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            currentCell = squareGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell, squareGrid.GetVertex(hit.point));
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }


    void ValidateDrag(SquareCell currentCell)
    {
        for (dragDirection = GridDirection.N;
            dragDirection <= GridDirection.NW;
            dragDirection++
        )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void HandleMousePosition()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            MoveEditorPointer(squareGrid.GetCell(hit.point), squareGrid.GetVertex(hit.point));
        }
    }

    public void SelectColor (int index)
    {
        activeColor = colors[index];
    }


    public void SetMode(int mode)
    {
        activeMode = (EditMode)mode;
    }


    public void ToggleCliffs()
    {
        allowCliffs = !allowCliffs;
    }

    public void SetBrushSize(float i)
    {
        pointerSize = (int)i;
    }

    void MoveEditorPointer(SquareCell cell, GridDirection vertex)
    {
        if (activeMode == EditMode.color || activeMode == EditMode.rivers)
        {
            pointerLocation = GridCoordinates.ToPosition(cell.coordinates) + Vector3.up * cell.CentreElevation * GridMetrics.elevationStep;
        }
        if (activeMode == EditMode.elevation)
        {
            pointerLocation = GridCoordinates.ToPosition(cell.coordinates) + GridMetrics.GetEdge(vertex) + Vector3.up * (int)cell.GridElevations[vertex] * GridMetrics.elevationStep;
        }
    }


    private void OnDrawGizmos()
    {
        if(pointerLocation != null)
        {
            Gizmos.color = Color.white;
            for (int x = 0; x < pointerSize; x++)
            {
                for (int z = 0; z < pointerSize; z++)
                {
                    Vector3 offPos = pointerLocation + new Vector3(x, 0, z);
                    if (activeMode == EditMode.elevation)
                    {
                        Gizmos.DrawSphere(offPos, 0.1f);
                    }
                    if (activeMode == EditMode.color || activeMode == EditMode.rivers)
                    {
                        Gizmos.DrawWireCube(offPos, new Vector3(1, 0, 1));
                    }
                }
            }

        }
    }


    void EditCells(SquareCell cell, GridDirection vertex)
    {
        for(int x=0; x < pointerSize; x++)
        {
            for(int z=0; z<pointerSize; z++)
            {
                Vector3 offPos = new Vector3(cell.coordinates.X + x, 0, cell.coordinates.Z + z);
                SquareCell offCell = squareGrid.GetCell(offPos);
                EditCell(offCell, vertex);
            }
        }
    }


    void EditCell(SquareCell cell, GridDirection vertex)
    {
        if(activeMode == EditMode.color)
        {
            cell.Color = activeColor;
        }
        else if(activeMode == EditMode.elevation)
        {
            if (Input.GetMouseButton(0) && (stopWatch.ElapsedMilliseconds > 500f || freshClick)) 
            {
                cell.ChangeVertexElevation(vertex, 1);
                if(!allowCliffs)
                {
                    if(cell.GetNeighbor(vertex)) cell.GetNeighbor(vertex).ChangeVertexElevation(vertex.Opposite(), 1);
                    if(cell.GetNeighbor(vertex.Next())) cell.GetNeighbor(vertex.Next()).ChangeVertexElevation(vertex.Previous2(), 1);
                    if(cell.GetNeighbor(vertex.Previous())) cell.GetNeighbor(vertex.Previous()).ChangeVertexElevation(vertex.Next2(), 1);
                }
                stopWatch.Reset();
                stopWatch.Start();
            }
            if (Input.GetMouseButton(1) && (stopWatch.ElapsedMilliseconds > 500f || freshClick))
            {
                cell.ChangeVertexElevation(vertex, -1);
                if (!allowCliffs)
                {
                    if (cell.GetNeighbor(vertex)) cell.GetNeighbor(vertex).ChangeVertexElevation(vertex.Opposite(), -1);
                    if (cell.GetNeighbor(vertex.Next())) cell.GetNeighbor(vertex.Next()).ChangeVertexElevation(vertex.Previous2(), -1);
                    if (cell.GetNeighbor(vertex.Previous())) cell.GetNeighbor(vertex.Previous()).ChangeVertexElevation(vertex.Next2(),-1);
                }
                stopWatch.Reset();
                stopWatch.Start();
            }
        }
        else if(activeMode == EditMode.rivers)
        {
            if(Input.GetMouseButton(1))
            {
                cell.RemoveRivers();
            }
            else if(isDrag)
            {
                UnityEngine.Debug.Log("is Drag");
                SquareCell otherCell = cell.GetNeighbor(dragDirection.Opposite()); // work with brushes
                if (otherCell)
                {
                    otherCell.SetOutgoingRiver(dragDirection);
                }
            }
        }
    }
}
