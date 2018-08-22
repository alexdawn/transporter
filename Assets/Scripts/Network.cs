using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Network : MonoBehaviour {

    public List<Spline> splines;
    public List<Node> nodes;
    private int nodeCounter;

    public void Reset()
    {
        splines = new List<Spline>();
        nodes = new List<Node>();
        nodeCounter = 0;
    }

    private int GetId()
    {
        return nodeCounter++;
    }

    public Node MakeNode(int id, Vector3 position)
    {
        Node node = new Node(id, position);
        nodes.Add(node);
        return node;
    }

    public Spline MakeLink()
    {
        Spline link = new Spline();
        Node anode = MakeNode(GetId(), transform.position);
        Node bnode = MakeNode(GetId(), transform.position + Vector3.forward);
        link.inNode = anode;
        link.outNode = bnode;
        anode.outLinks.Add(link);
        bnode.inLinks.Add(link);
        splines.Add(link);
        return link;
    }

    public Spline MakeLink(Node anode, Node bnode)
    {
        Spline link = new Spline();
        link.inNode = anode;
        link.outNode = bnode;
        anode.outLinks.Add(link);
        bnode.inLinks.Add(link);
        splines.Add(link);
        return link;
    }

    public Spline MakeLink(Node connectingNode, bool isSource, Vector3 newPosition)
    {
        if (isSource)
        {
            Node bnode = MakeNode(GetId(), newPosition);
            return MakeLink(connectingNode, bnode);
        }
        else
        {
            Node anode = MakeNode(GetId(), newPosition);
            return MakeLink(anode, connectingNode);
        }
    }
}

public class BezierSp
{

    [SerializeField]
    private Vector3[] points;
    [SerializeField]
    private BezierControlPointMode[] modes;
    [SerializeField]
    private bool loop;
    [SerializeField]
    private int lookupResolution;
    private Vector3[] pointsLookup;
    private float[] distanceLookup;

    public BezierSp(bool Loop)
    {
        loop = Loop;
        lookupResolution = 100;
        Reset();
    }

    public int LookupResolution
    {
        get { return lookupResolution * (points.Length / 3); }
        set
        {
            lookupResolution = value;
            CalculateLookups();
        }
    }

    public bool Loop
    {
        get
        {
            return loop;
        }
        set
        {
            loop = value;
            if (value == true)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (loop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }
        points[index] = point;
        EnforceMode(index);
        CalculateLookups();
    }

    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if (loop)
        {
            if (modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if (modeIndex == modes.Length - 1)
            {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }
        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public void Reset()
    {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
        CalculateLookups();
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        if (i + 3 >= points.Length || i < 0)
        {
            Debug.Log("Index is out of bounds " + i + " " + t);
        }
        return Bezier.GetPoint(
            points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }

    public Vector3 GetLinearPoint(float x)
    {
        float dist = x * distanceLookup[distanceLookup.Length - 1];
        int i = 0;
        while (distanceLookup[i] < dist)
        {
            i++;
        }
        //Debug.Log("distance found" + i + " " + distanceLookup[i] + " " + dist);
        if (i == 0)
        {
            return pointsLookup[0];
        }
        float interpolationFrac = (distanceLookup[i] - dist) / (distanceLookup[i] - distanceLookup[i - 1]);
        //Debug.Log(x + " " + i + " " + interpolationFrac);
        return Vector3.Lerp(pointsLookup[i], pointsLookup[i - 1], interpolationFrac);
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return Bezier.GetFirstDerivative(
            points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public void AddCurve()
    {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
        CalculateLookups();
    }

    public void CalculateLookups()
    {
        Array.Resize(ref pointsLookup, LookupResolution + 1);
        Array.Resize(ref distanceLookup, LookupResolution + 1);
        for (int i = 0; i <= LookupResolution; i++)
        {
            //Debug.Log(i + " " + LookupResolution);
            pointsLookup[i] = GetPoint(i / (float)LookupResolution);
            CalculateLinearDistance(i);
        }
    }

    public void CalculateLinearDistance(int i)
    {
        if (i == 0)
        {
            distanceLookup[i] = 0;
        }
        else
        {
            distanceLookup[i] = distanceLookup[i - 1] + Vector3.Magnitude(pointsLookup[i] - pointsLookup[i - 1]);
        }
    }
}

public class Spline
{
    public bool isBezier;
    public Node inNode;
    public Node outNode;
    public BezierSp curve;

    public Spline()
    {
        curve = new BezierSp(false); //TODO if in==out then loop is true
    }

    public void UpdateStartPoints()
    {
        curve.SetControlPoint(0, inNode.Location);
        curve.SetControlPoint(1, inNode.InControl);
    }
    public void UpdateEndPoints()
    {
        curve.SetControlPoint(curve.ControlPointCount - 1, outNode.Location);
        curve.SetControlPoint(curve.ControlPointCount - 2, outNode.OutControl);
    }
}

public enum NodeType
{
    Junction,
    TurnPoint,
    WaitPoint
}

public class Node
{
    public int id;
    public NodeType type;
    public BezierControlPointMode ControlMode;
    public List<Spline> inLinks;
    public List<Spline> outLinks;
    [SerializeField]
    private Vector3 location;
    [SerializeField]
    private Vector3 inControl;
    [SerializeField]
    private Vector3 outControl;

    public Node(int Id, Vector3 Position)
    {
        id = Id;
        location = Position;
        inLinks = new List<Spline>();
        outLinks = new List<Spline>();
        inControl = Position + Vector3.back;
        outControl = Position + Vector3.forward;
    }

    public Vector3 Location
    {
        get { return location; }
        set
        {
            location = value;
            UpdateIn();
            UpdateOut();
        }
    }

    public Vector3 InControl
    {
        get { return location; }
        set
        {
            inControl = value;
            UpdateIn();
        }
    }

    public Vector3 OutControl
    {
        get { return location; }
        set
        {
            outControl = value;
            UpdateOut();
        }
    }

    private void UpdateIn()
    {
        foreach (Spline link in inLinks)
        {
            link.UpdateEndPoints();
        }
    }

    private void UpdateOut()
    {
        foreach (Spline link in outLinks)
        {
            link.UpdateEndPoints();
        }
    }
}
