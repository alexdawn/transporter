using UnityEngine;

[System.Serializable]
public struct GridMetrics
{
    public const float elevationStep = 2f / 9f;
    public const float gridSize = 1f;
    public const float halfSize = gridSize / 2;
    public const int chunkSizeX = 16, chunkSizeZ = 16;
    public const float solidFactor = 0.8f;
    public const float blendFactor = 1f - solidFactor;
    public const float streamBedElevationOffset = -1f;
    public const float waterElevationOffset = -0.4f;

    public const int hashGridSize = 256;

    static GridHash[] hashGrid;

    public static void InitializeHashGrid(int seed)
    {
        hashGrid = new GridHash[hashGridSize * hashGridSize];
        Random.State currentState = Random.state;
        Random.InitState(seed);
        for (int i = 0; i < hashGrid.Length; i++)
        {
            hashGrid[i] = GridHash.Create();
        }
        Random.state = currentState;
    }

    public static GridHash SampleHashGrid(Vector3 position)
    {
        int x = (int)position.x % hashGridSize;
        if (x < 0)
        {
            x += hashGridSize;
        }
        int z = (int)position.z % hashGridSize;
        if (z < 0)
        {
            z += hashGridSize;
        }
        return hashGrid[x + z * hashGridSize];
    }


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