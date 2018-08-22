using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLine : MonoBehaviour {
    public List<PathLine> Sources { get; private set; }
    public List<PathLine> Destinations { get; private set; }
    public BezierSpline Line { get; private set; }

    public void Setup(PathLine parent)
    {
        Line = gameObject.AddComponent<BezierSpline>();
        if (parent != null)
        {
            Sources.Add(parent);
        }
    }

    public void AddDest(PathLine child)
    {
        Destinations.Add(child);
    }
}
