using UnityEngine;

public struct GridHash
{
    public float a, b;

    public static GridHash Create()
    {
        GridHash hash;
        hash.a = Random.value;
        hash.b = Random.value;
        return hash;
    }
}
