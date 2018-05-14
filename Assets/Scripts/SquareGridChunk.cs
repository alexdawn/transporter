using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareGridChunk : MonoBehaviour {
    SquareCell[] cells;

    GridMesh gridMesh;
    Canvas gridCanvas;


    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        gridMesh = GetComponentInChildren<GridMesh>();

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
        gridMesh.Triangulate(cells);
        enabled = false;
    }
}
