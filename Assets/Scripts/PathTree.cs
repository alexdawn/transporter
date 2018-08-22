using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTree : MonoBehaviour
{
    List<PathLine> segments;

    public List<PathLine> Segments
    {
        get { return segments; }
    }

    public PathLine AddSegment(PathLine parent)
    {
        PathLine newSegment = gameObject.AddComponent<PathLine>();
        newSegment.Setup(parent);
        if (parent != null)
        {
            newSegment.Line.SetControlPoint(0, parent.Line.GetControlPoint(parent.Line.ControlPointCount - 1));
            parent.AddDest(newSegment);
        }
        segments.Add(newSegment);
        return newSegment;
    }
}