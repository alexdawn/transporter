using UnityEngine;

[System.Serializable]
public struct GridMetrics
{
    public const float elevationStep = 2f / 9f;
    public const float gridSize = 1f;
    public const float halfSize = gridSize / 2;
    public const int chunkSizeX = 5, chunkSizeZ = 5;
    public const float solidFactor = 0.8f;
    public const float blendFactor = 1f - solidFactor;
    public const float streamBedElevationOffset = -1f;
    public const float riverSurfaceElevationOffset = -0.5f;


    static readonly Vector3[] directions = {
        new Vector3(0, 0, halfSize),
        new Vector3(halfSize, 0, halfSize),
        new Vector3(halfSize, 0, 0),
        new Vector3(halfSize, 0, -halfSize),
        new Vector3(0, 0, -halfSize),
        new Vector3(-halfSize, 0, -halfSize),
        new Vector3(-halfSize, 0, 0),
        new Vector3(-halfSize, 0, halfSize),
    };


    public static float DiagSize()
    {
        return Mathf.Sqrt(2);
    }
        

    public static Vector3 GetEdge(GridDirection direction)
    {
        return directions[(int)direction];
    }


    public static Vector3 GetSolidEdge(GridDirection direction)
    {
        return directions[(int)direction] * solidFactor;
    }


    public static Vector3 GetBridge(GridDirection direction)
    {
        return directions[(int)direction] * blendFactor;
    }
}