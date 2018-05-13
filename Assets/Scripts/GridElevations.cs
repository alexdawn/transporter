using UnityEngine;

[System.Serializable]
public struct GridElevations
{
    [SerializeField]
    private int y0, y1, y2, y3;

    public const int maxHeight = 10;
    public const int chunkSize = 16;
    public const float perlinScale = 1f;

    public int Y0
    {
        get { return y0; }
    }
    public int Y1
    {
        get { return y1; }
    }
    public int Y2
    {
        get { return y2; }
    }
    public int Y3
    {
        get { return y3; }
    }

    public GridElevations(int y0, int y1, int y2, int y3)
    {
        this.y0 = y0;
        this.y1 = y1;
        this.y2 = y2;
        this.y3 = y3;
    }


    public static int GetTerrainHeight(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        return (int)(maxHeight * Mathf.PerlinNoise((x / (float)chunkSize) * perlinScale, (z / (float)chunkSize) * perlinScale));
    }


    public static GridElevations GetVertexHeights(Vector3 position)
    {
        int heightCentre = GetTerrainHeight(position);
        int v0 = GetTerrainHeight(GridMetrics.GetBottomLeftVertex(position)) - heightCentre;
        int v1 = GetTerrainHeight(GridMetrics.GetTopLeftVertex(position)) - heightCentre;
        int v2 = GetTerrainHeight(GridMetrics.GetTopRightVertex(position)) - heightCentre;
        int v3 = GetTerrainHeight(GridMetrics.GetBottomRightVertex(position)) - heightCentre;
        return new GridElevations(v0, v1, v2, v3);
    }


    public override string ToString()
    {
        return string.Format("({0}, {1}, {2}, {3})", y0.ToString(), y1.ToString(), y2.ToString(), y3.ToString());
    }
}