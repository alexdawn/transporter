using UnityEngine;
using UnityEngine.UI;
using System;

public class SquareGrid : MonoBehaviour
{

    public int chunkCountX, chunkCountZ;
    int cellCountX, cellCountZ;
    public bool showLabels;
    public SquareCell cellPrefab;
    public SquareGridChunk chunkPrefab;
    public Text cellLabelPrefab;
    public int seed;
    CullingGroup chunkCuller;
    BoundingSphere[] spheres;

    SquareGridChunk[] chunks;
    SquareCell[] cells;
    GroundMaterial[] gridMaterials;

    private void StateChangedMethod(CullingGroupEvent evt)
    {
        if (evt.hasBecomeVisible)
        {
            //Debug.Log("Sphere " + evt.index + " has become visible!");
            chunks[evt.index].gameObject.SetActive(true);
        }
        else if (evt.hasBecomeInvisible)
        {
            //Debug.Log("Sphere " + evt.index + " has become invisible!");
            chunks[evt.index].gameObject.SetActive(false);
        }

        //if(evt.currentDistance != evt.previousDistance)
        //{
            if(evt.currentDistance < 1)
            {
                //Debug.Log("Sphere " + evt.index + " has moved to band 0!");
                //chunks[evt.index].gameObject.GetComponentInChildren<GridFeatureManager>().gameObject.SetActive(true);
            }
            else if (evt.currentDistance >= 1)
            {
                //Debug.Log("Sphere " + evt.index + " has moved out of band 0!");
                //chunks[evt.index].gameObject.GetComponentInChildren<GridFeatureManager>().gameObject.SetActive(false);
            }
        //}
    }

    private void OnDrawGizmos()
    {
        for(int i=0; i< chunkCountX * chunkCountZ; i++)
        {
                Gizmos.DrawWireSphere(spheres[i].position, spheres[i].radius);
        }
    }

    private void Awake()
    {
        gridMaterials = GameObject.Find("EditorCanvas").GetComponent<MapEditor>().materials;
        cellCountX = chunkCountX * GridMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * GridMetrics.chunkSizeZ;
        GridMetrics.InitializeHashGrid(seed);
        chunkCuller = new CullingGroup();
        chunkCuller.SetBoundingDistances(new float[] { 30f, 60f, 90f});
        chunkCuller.targetCamera = Camera.main;
        chunkCuller.onStateChanged = StateChangedMethod;

        CreateChunks();
        CreateCells();
    }

    void OnDestroy()
    {
        chunkCuller.Dispose();
        chunkCuller = null;
    }


    void OnEnable()
    {
        GridMetrics.InitializeHashGrid(seed);
    }

    void CreateChunks()
    {
        chunks = new SquareGridChunk[chunkCountX * chunkCountZ];
        spheres = new BoundingSphere[chunkCountX * chunkCountZ];

        for (int z=0, i=0; z< chunkCountZ; z++)
        {
            for(int x=0; x<chunkCountX; x++)
            {
                SquareGridChunk chunk = chunks[i] = Instantiate(chunkPrefab);
                spheres[i] = new BoundingSphere(new Vector3((x + 0.5f) * GridMetrics.chunkSizeX, 0, (z + 0.5f) * GridMetrics.chunkSizeZ), Mathf.Max(GridMetrics.chunkSizeX, GridMetrics.chunkSizeZ));
                i++;
                chunk.transform.SetParent(transform);
            }
        }
        chunkCuller.SetBoundingSpheres(spheres);
        chunkCuller.SetBoundingSphereCount(chunkCountX * chunkCountZ);
    }

    void CreateCells()
    {
        cells = new SquareCell[cellCountZ * cellCountX];
        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
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
        cell.transform.localPosition = position;
        if (x > 0)
        {
            cell.SetNeighbor(GridDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            cell.SetNeighbor(GridDirection.S, cells[i - cellCountX]);
        }
        if (z > 0 && x > 0)
        {
            cell.SetNeighbor(GridDirection.SW, cells[i - cellCountX - 1]);
        }
        if (z > 0 && x < cellCountX - 1)
        {
            cell.SetNeighbor(GridDirection.SE, cells[i - cellCountX + 1]);
        }
        cell.coordinates = GridCoordinates.FromOffsetCoordinates(x, z);
        cell.GridElevations = GridElevations.GetVertexHeights(position, seed);
        // start off with grass everywhere
        cell.Tile = gridMaterials[0].GetClone; //gridColors[(int)((cell.CentreElevation / (float)GridElevations.maxHeight) * gridColors.Length)];
        if(cell.CentreElevation < 7) // basic treeline cut-off
        {
            cell.PlantLevel = UnityEngine.Random.Range(0, 10) - 4;
        }
        if (showLabels)
        {
            label.rectTransform.anchoredPosition = new Vector2(x, z);
            label.text = x.ToString() + ", " + z.ToString();
        }
        AddCellToChunk(x, z, cell);
    }


    void AddCellToChunk(int x, int z, SquareCell cell)
    {
        int chunkX = x / GridMetrics.chunkSizeX;
        int chunkZ = z / GridMetrics.chunkSizeZ;
        SquareGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * GridMetrics.chunkSizeX;
        int localZ = z - chunkZ * GridMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * GridMetrics.chunkSizeX, cell);
    }


    public SquareCell GetCell(Vector3 position)
    {
        // get the cell at position
        position = transform.InverseTransformPoint(position);
        GridCoordinates coordinates = GridCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX;
        return cells[index];
    }

    public SquareCell GetCellOffset(Vector3 position, int x, int z)
        // returns a cell instance with a set offset from position
    {
        position = transform.InverseTransformPoint(position);
        GridCoordinates coordinates = GridCoordinates.FromPosition(position);
        int index = coordinates.X + x + (coordinates.Z + z) * cellCountX;
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

    public GridDirection GetEdge(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        position.x -= 0.5f;
        position.z -= 0.5f;
        float fracX = position.x - Mathf.Floor(position.x);
        float fracZ = position.z - Mathf.Floor(position.z);
        if (fracZ >= 0.5f && Mathf.Abs(fracZ - 0.5f) > Mathf.Abs(fracX - 0.5f))
        {
            return GridDirection.N;
        }
        else if (fracX >= 0.5f && Mathf.Abs(fracZ - 0.5f) < Mathf.Abs(fracX - 0.5f))
        {
            return GridDirection.E;
        }
        else if (fracZ < 0.5f && Mathf.Abs(fracZ - 0.5f) > Mathf.Abs(fracX - 0.5f))
        {
            return GridDirection.S;
        }
        else if (fracX < 0.5f && Mathf.Abs(fracZ - 0.5f) < Mathf.Abs(fracX - 0.5f))
        {
            return GridDirection.W;
        }
        else
        {
            Debug.Log(string.Format("invalid result {0} {1}", fracX, fracZ));
            throw new Exception("Can't determin vertex");
        }
    }
}