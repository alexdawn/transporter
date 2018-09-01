using UnityEngine;

[System.Serializable]
public struct GridElevations
{
    [SerializeField]
    private int y0, y1, y2, y3;

    public const int maxHeight = 50;
    public const int chunkSize = 16;
    public const float perlinScale = 1f;

    public int Y0
    {
        get { return y0; }
        set { y0 = value; }
    }
    public int Y1
    {
        get { return y1; }
        set { y1 = value; }
    }
    public int Y2
    {
        get { return y2; }
        set { y2 = value; }
    }
    public int Y3
    {
        get { return y3; }
        set { y3 = value; }
    }

    public int? this[GridDirection i]
    {
        get
        {
            switch (i)
            {
                case GridDirection.SW: return y0;
                case GridDirection.NW: return y1;
                case GridDirection.NE: return y2;
                case GridDirection.SE: return y3;
                default: return null;
            }
        }
        set
        {
            switch (i)
            {
                case GridDirection.SW: y0 = (int)value; break;
                case GridDirection.NW: y1 = (int)value; break;
                case GridDirection.NE: y2 = (int)value; break;
                case GridDirection.SE: y3 = (int)value; break;
                default: break;
            }
        }
    }

    public GridElevations(int y0, int y1, int y2, int y3)
    {
        this.y0 = y0;
        this.y1 = y1;
        this.y2 = y2;
        this.y3 = y3;
    }


    public static int GetTerrainHeightFromPerlin(Vector3 position, int seed)
    {
        float x = position.x;
        float z = position.z;
        return (int)(maxHeight * Mathf.PerlinNoise((x / (float)chunkSize) * perlinScale + seed, (z / (float)chunkSize) * perlinScale + seed));
    }

    public static int GetTerrainHeightFromMap(Vector3 position, Texture2D heightmap)
    {
        return (int)(maxHeight * heightmap.GetPixel((int)(position.x + 0.5f), (int)(position.z + 0.5f)).grayscale);
    }


    public static GridElevations GetVertexHeights(Vector3 position, int seed)
    {
        int v0 = GetTerrainHeightFromPerlin(position + GridMetrics.GetEdge(GridDirection.SW), seed);
        int v1 = GetTerrainHeightFromPerlin(position + GridMetrics.GetEdge(GridDirection.NW), seed);
        int v2 = GetTerrainHeightFromPerlin(position + GridMetrics.GetEdge(GridDirection.NE), seed);
        int v3 = GetTerrainHeightFromPerlin(position + GridMetrics.GetEdge(GridDirection.SE), seed);
        return new GridElevations(v0, v1, v2, v3);
    }

    public static GridElevations GetVertexHeightsFromHeightmap(Vector3 position, Texture2D heightmap)
    {
        int v0 = GetTerrainHeightFromMap(position + GridMetrics.GetEdge(GridDirection.SW), heightmap);
        int v1 = GetTerrainHeightFromMap(position + GridMetrics.GetEdge(GridDirection.NW), heightmap);
        int v2 = GetTerrainHeightFromMap(position + GridMetrics.GetEdge(GridDirection.NE), heightmap);
        int v3 = GetTerrainHeightFromMap(position + GridMetrics.GetEdge(GridDirection.SE), heightmap);
        return new GridElevations(v0, v1, v2, v3);
    }


    public override string ToString()
    {
        return string.Format("({0}, {1}, {2}, {3})", y0.ToString(), y1.ToString(), y2.ToString(), y3.ToString());
    }
}