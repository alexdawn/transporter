using UnityEngine;

[System.Serializable]
public struct GridMetrics
{
    public const float elevationStep = 1f / 2f;
    public const float gridSize = 1f;


    public static Vector3 GetBottomLeftVertex(Vector3 centre)
    {
        return centre + new Vector3(-gridSize/2, 0, -gridSize / 2);
    }


    public static Vector3 GetTopLeftVertex(Vector3 centre)
    {
        return centre + new Vector3(-gridSize / 2, 0, gridSize / 2);
    }


    public static Vector3 GetTopRightVertex(Vector3 centre)
    {
        return centre + new Vector3(gridSize / 2, 0, gridSize / 2);
    }


    public static Vector3 GetBottomRightVertex(Vector3 centre)
    {
        return centre + new Vector3(gridSize / 2, 0, -gridSize / 2);
    }
}