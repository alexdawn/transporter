using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFeatureManager : MonoBehaviour {
    public FeatureList[] featurePrefabs;
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
        if (cell.UrbanLevel == 0)
        {
            return;
        }
        int randomFeature = (int)Mathf.Floor(featurePrefabs[cell.UrbanLevel - 1].Length * hash.a);
        Transform instance = Instantiate(featurePrefabs[cell.UrbanLevel-1][randomFeature]);
        instance.localPosition = position;
        instance.localRotation = Quaternion.Euler(-90, 90 * Mathf.Round(hash.b * 4), 0);
        instance.SetParent(container, false);
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