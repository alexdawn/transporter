using UnityEngine;
using UnityEngine.UI;
using System;

public class SquareGrid : MonoBehaviour
{

    public int chunkCountX, chunkCountZ;
    int cellCountX, cellCountZ;
    public SquareCell cellPrefab;
    public SquareGridChunk chunkPrefab;
    public Texture2D heightmap;
    public int seed;
    public float distanceBand1;
    public float distanceBand2;
    public float distanceBand3;
    CullingGroup chunkCuller;
    BoundingSphere[] spheres;

    SquareGridChunk[] chunks;
    SquareCell[] cells;
    GroundMaterial[] gridMaterials;

    private void StateChangedMethod(CullingGroupEvent evt)
    {
        if (evt.hasBecomeVisible)
        {
            chunks[evt.index].gameObject.SetActive(true);
        }
        else if (evt.hasBecomeInvisible)
        {
            chunks[evt.index].gameObject.SetActive(false);
        }
        if(evt.currentDistance < 1)
        {
            //SafeSet(evt.index, true);
        }
        else if (evt.currentDistance >= 1)
        {
            //SafeSet(evt.index, false);
        }
    }

   private void SafeSet(int i, bool state)
    {
        GameObject chunk = chunks[i].gameObject;
        GridFeatureManager manager = chunk.GetComponentInChildren<GridFeatureManager>();
        if (manager != null)
            manager.gameObject.SetActive(state);
    }

    // use for setting initial state
    private void UpdateCullingChunk(int i)
    {
        if (chunkCuller.IsVisible(i))
        {
            chunks[i].gameObject.SetActive(true);
        }
        else
        {
            chunks[i].gameObject.SetActive(false);
        }
        if (chunkCuller.GetDistance(i) < 1)
        {
            //SafeSet(i, true);
        }
        else
        {
            //SafeSet(i, false);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < chunkCountX * chunkCountZ; i++)
            {
                if (chunkCuller.IsVisible(i) == false)
                    Gizmos.color = Color.grey;
                else
                {
                    switch (chunkCuller.GetDistance(i))
                    {
                        case 0: Gizmos.color = Color.red; break;
                        case 1: Gizmos.color = Color.green; break;
                        case 2: Gizmos.color = Color.blue; break;

                    }
                }
                Gizmos.DrawWireSphere(spheres[i].position, spheres[i].radius);
            }
        }
    }

    private void Awake()
    {
        gridMaterials = GameObject.Find("EditorCanvas").GetComponent<MapEditor>().materials;
        cellCountX = chunkCountX * GridMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * GridMetrics.chunkSizeZ;
        GridMetrics.InitializeHashGrid(seed);
        chunkCuller = new CullingGroup();
        chunkCuller.SetBoundingDistances(new float[] { distanceBand1, distanceBand2, distanceBand3});
        chunkCuller.onStateChanged = StateChangedMethod;

        CreateChunks();
        CreateCells();
    }

    private void Start()
    {
        GameObject cam = GameObject.Find("Main Camera");
        chunkCuller.targetCamera = cam.GetComponent<Camera>();
        chunkCuller.SetDistanceReferencePoint(cam.transform);
        for(int i=0; i< chunks.Length; i++)
        {
            UpdateCullingChunk(i);
        }
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
        if(heightmap == null)
        {
            cell.GridElevations = GridElevations.GetVertexHeights(position, seed);
        }
        else
        {
            cell.GridElevations = GridElevations.GetVertexHeightsFromHeightmap(position, heightmap);
        }

        // start off with grass everywhere
        cell.Tile = gridMaterials[0].GetClone;
        if(cell.CentreElevation < 7) // basic treeline cut-off
        {
            cell.PlantLevel = UnityEngine.Random.Range(0, 100) - 95;
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
            throw new Exception("Can't determin grid vertex");
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
            throw new Exception("Can't determin vertex");
        }
    }
}