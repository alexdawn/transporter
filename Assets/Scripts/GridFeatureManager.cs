using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFeatureManager : MonoBehaviour {
    public FeatureList[] featurePrefabs, farmPrefabs, treePrefabs;
    Transform container;

    public void Clear() {
        if (container)
        {
            Destroy(container.gameObject);
        }
        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
    }

    public void Apply() { }

    public void AddFeature(SquareCell cell, Vector3 position) {
        GridHash hash = GridMetrics.SampleHashGrid(position);
        if (cell.UrbanLevel != 0)
        {
            int randomFeature = (int)Mathf.Floor(featurePrefabs[cell.UrbanLevel - 1].Length * hash.a);
            Transform instance = Instantiate(featurePrefabs[cell.UrbanLevel - 1][randomFeature]);
            instance.localPosition = position;
            instance.localRotation = instance.localRotation * Quaternion.Euler(0, 90 * Mathf.Round(hash.b * 4), 0);
            instance.SetParent(container, false);
        }
        else if (cell.FarmLevel != 0)
        {

        }
        else if (cell.PlantLevel != 0)
        {
            for (int i = 0; i < cell.PlantLevel; i++)
            {
                float selectFeature = Mathf.Clamp(Mathf.PerlinNoise((position.x + i / 6f) * 100, (position.z + i / 6f) * 100), 0f ,1f);
                int randomFeature = (int)Mathf.Floor(treePrefabs[0].Length * selectFeature);
                Transform instance = Instantiate(treePrefabs[0][randomFeature]);
                instance.localPosition = position + (Quaternion.Euler(0, i / (float)cell.PlantLevel * 360f, 0) * new Vector3(hash.a * 0.5f, 0, 0));
                instance.localRotation = Quaternion.Euler(0f, 360f * hash.a, 0f);
                instance.SetParent(container, false);
            }

        }

    }

    Vector3 Perturb(Vector3 position)
    {
        float squareSize = 16f;
        float magnitude = 0.25f;
        float perlinScale = 100f;
        float zOffset = 130f;
        Vector3 newPosition = position;
        newPosition.x += magnitude * Mathf.PerlinNoise((position.x / squareSize) * perlinScale, (position.z / squareSize) * perlinScale);
        newPosition.z += magnitude * Mathf.PerlinNoise(((position.x + zOffset) / squareSize) * perlinScale, ((position.z + zOffset) / squareSize) * perlinScale);
        return newPosition;

    }
}


[System.Serializable]
public class FeatureList
{
    public Transform[] features;

    public int Length
        {
        get { return features.Length; }
        }

    public Transform this[int param]
    {
        get { return features[param]; }
    }
}