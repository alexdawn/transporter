using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFeatureManager : MonoBehaviour {
    public FeatureList[] featurePrefabs, farmPrefabs, treePrefabs;
    public Building[] scenaryBuildings;
    public Industry[] industryBuildings;
    public Building[] townBuildings;
    public Building[] farmBuildings;
    Transform container;
    Transform containerNoDestroy;

    public void Awake()
    {
        containerNoDestroy = new GameObject("Container No Destroy").transform;
        containerNoDestroy.SetParent(transform, false);
    }

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
        if (cell.UrbanLevel != 0 && cell.BuildingOnSquare == null)
        {
            int buildingIndex = Random.Range(0, townBuildings.Length);
            cell.BuildingOnSquare = Instantiate(townBuildings[buildingIndex]);
            cell.BuildingOnSquare.Construct(cell, position, hash);
            cell.BuildingOnSquare.BuildingModel.SetParent(containerNoDestroy, false);
        }
        else if (cell.Industry != 0 && cell.BuildingOnSquare == null)
        {
            cell.BuildingOnSquare = Instantiate(industryBuildings[cell.Industry-1]);
            cell.BuildingOnSquare.Construct(cell, position, hash);
            cell.BuildingOnSquare.BuildingModel.SetParent(containerNoDestroy, false);
        }
        else if (cell.FarmLevel != 0)
        {

        }
        else if (cell.PlantLevel != 0)
        {
            for (int i = 0; i < cell.PlantLevel; i++)
            {
                float selectFeature = Mathf.Clamp(Mathf.PerlinNoise((position.x + i / 6f) * 100, (position.z + i / 6f) * 100), 0f ,1f);
                int randomFeature = (int)Mathf.Floor((treePrefabs[0].Length - 1) * selectFeature);
                Transform instance = Instantiate(treePrefabs[0][randomFeature]);
                instance.localPosition = position + (Quaternion.Euler(0, i / (float)cell.PlantLevel * 360f, 0) * new Vector3(hash.a * 0.3f + 0.2f, 0, 0));
                instance.localRotation = Quaternion.Euler(0f, 360f * hash.a, 0f);
                instance.SetParent(container, false);
            }

        }
        else if(cell.ScenaryObject != 0 && cell.BuildingOnSquare == null)
        {
            cell.BuildingOnSquare = Instantiate(scenaryBuildings[cell.ScenaryObject - 1]);
            cell.BuildingOnSquare.Construct(cell, position, hash);
            cell.BuildingOnSquare.BuildingModel.SetParent(containerNoDestroy, false);
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

[System.Serializable]
public class BuildingList
{
    public Building[] features;

    public int Length
    {
        get { return features.Length; }
    }

    public Building this[int param]
    {
        get { return features[param]; }
    }
}