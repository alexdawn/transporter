using UnityEngine;

[System.Serializable]
public struct GridMetrics
{
    public const float elevationStep = 1f / 2f;
    public const float gridSize = 1f;
    public const float halfSize = gridSize / 2;

    static Vector3[] directions = {
        new Vector3(0, 0, halfSize),
        new Vector3(halfSize, 0, halfSize),
        new Vector3(halfSize, 0, 0),
        new Vector3(halfSize, 0, -halfSize),
        new Vector3(0, 0, -halfSize),
        new Vector3(-halfSize, 0, -halfSize),
        new Vector3(-halfSize, 0, 0),
        new Vector3(-halfSize, 0, halfSize),
    };

    public static Vector3 GetEdge(GridDirection direction)
    {
        return directions[(int)direction];
    }

    public static Vector3 GetBottomLeftVertex(Vector3 centre)
    {
        return centre + GetEdge(GridDirection.SW);
    }


    public static Vector3 GetTopLeftVertex(Vector3 centre)
    {
        return centre + GetEdge(GridDirection.NW);
    }


    public static Vector3 GetTopRightVertex(Vector3 centre)
    {
        return centre + GetEdge(GridDirection.NE);
    }


    public static Vector3 GetBottomRightVertex(Vector3 centre)
    {
        return centre + GetEdge(GridDirection.SE);
    }
}