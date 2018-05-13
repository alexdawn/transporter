using UnityEngine;
using UnityEngine.UI;
using System;

public class SquareGrid : MonoBehaviour
{

    public int width = 6;
    public int height = 6;
    public SquareCell cellPrefab;
    public Text cellLabelPrefab;

    SquareCell[] cells;
    Canvas gridCanvas;
    GridMesh gridMesh;
    Color[] gridColors;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        gridMesh = GetComponentInChildren<GridMesh>();
        gridColors = GameObject.Find("EditorCanvas").GetComponent<MapEditor>().colors;
        cells = new SquareCell[height * width];
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }


    void Start()
    {
        gridMesh.Triangulate(cells);
    }


    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = x;
        position.y = 0f;
        position.z = z;

        SquareCell cell = cells[i] = Instantiate<SquareCell>(cellPrefab);
        Text label = Instantiate<Text>(cellLabelPrefab);
        cell.uiRect = label.rectTransform;
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        if (x > 0)
        {
            cell.SetNeighbor(GridDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            cell.SetNeighbor(GridDirection.S, cells[i - width]);
        }
        if (z > 0 && x > 0)
        {
            cell.SetNeighbor(GridDirection.SW, cells[i - width - 1]);
        }
        if (z > 0 && x < width - 1)
        {
            cell.SetNeighbor(GridDirection.SE, cells[i - width + 1]);
        }
        cell.coordinates = GridCoordinates.FromOffsetCoordinates(x, z);
        cell.GridElevations = GridElevations.GetVertexHeights(position);
        cell.color = gridColors[(int)((cell.CentreElevation / (float)GridElevations.maxHeight) * gridColors.Length)];
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(x, z);
        label.text = x.ToString() + ", " + z.ToString();
    }


    public SquareCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        GridCoordinates coordinates = GridCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width;
        return cells[index];
    }


    public GridDirection GetVertex(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        float fracX = position.x - Mathf.Floor(position.x);
        float fracZ = position.z - Mathf.Floor(position.z);
        if (fracX < 0.5f && fracZ < 0.5f)
        {
            return GridDirection.NE;
        }
        else if(fracX < 0.5f && fracZ >= 0.5f)
        {
            return GridDirection.SE;
        }
        else if (fracX >= 0.5f && fracZ >= 0.5f)
        {
            return GridDirection.SW;
        }
        else if (fracX >= 0.5f && fracZ < 0.5f)
        {
            return GridDirection.NW;
        }
        else
        {
            Debug.Log(string.Format("invalid result {0} {1}", fracX, fracZ));
            throw new Exception("Can't determin vertex");
        }
    }

    public void Refresh()
    {
        gridMesh.Triangulate(cells);
    }
}