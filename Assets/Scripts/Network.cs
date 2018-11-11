using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Network : MonoBehaviour, ISerializationCallbackReceiver {

    public List<Spline> splines;
    public List<Node> nodes;
    [SerializeField]
    private int nodeCounter;
    public List<SerializableNode> serializedNodes;
    public List<SerializableSpline> serializedSplines;

    [Serializable]
    public struct SerializableNode
    {
        public int id;
        public NodeType type;
        public float waitTime;
        public BezierControlPointMode ControlMode;
        public NodeDirection boundDirect;
        public GridDirection compassDirection;
        public Vector3 location;
        public Vector3 inControl;
        public Vector3 outControl;
        public List<int> inLinkANode;
        public List<int> inLinkBNode;
        public List<int> outLinkANode;
        public List<int> outLinkBNode;
    }
    [Serializable]
    public struct SerializableSpline
    {
        public bool isBezier;
        public int inNodeId;
        public int outNodeId;
        public BezierSp curve;
    }

    public void Start()
    {
        foreach(Spline s in splines)
        {
            s.curve.CalculateLookups();
        }
    }

    public void JoinNetwork(Network network)
    {
        Duplicate(network.nodes);
    }

    public void OnBeforeSerialize()
    {
        if (serializedNodes == null) serializedNodes = new List<SerializableNode>();
        serializedNodes.Clear();
        foreach(Node n in nodes)
        {
            AddNodeToSerializedNodes(n);
        }
        serializedSplines.Clear();
        foreach(Spline s in splines)
        {
            AddSplineToSerializedSpline(s);
        }
    }
    void AddNodeToSerializedNodes(Node n)
    {
        SerializableNode serializedNode = new SerializableNode()
        {
            id = n.id,
            type = n.type,
            waitTime = n.waitTime,
            ControlMode = n.ControlMode,
            boundDirect = n.boundDirect,
            compassDirection = n.compassDirection,
            location = n.Location,
            inControl = n.InControl,
            outControl = n.OutControl,
            inLinkANode = new List<int>(),
            inLinkBNode = new List<int>(),
            outLinkANode = new List<int>(),
            outLinkBNode = new List<int>()
        }
        ;
        foreach (Spline spline in n.inLinks)
        {
            serializedNode.inLinkANode.Add(spline.inNode.id);
            serializedNode.inLinkBNode.Add(spline.outNode.id);
        }
        foreach (Spline spline in n.outLinks)
        {
            serializedNode.outLinkANode.Add(spline.inNode.id);
            serializedNode.outLinkBNode.Add(spline.outNode.id);
        }
        serializedNodes.Add(serializedNode);
    }
    public void AddSplineToSerializedSpline(Spline s)
    {
        SerializableSpline serializedSpline = new SerializableSpline()
        {
            isBezier = s.isBezier,
            inNodeId = s.inNode.id,
            outNodeId = s.outNode.id,
            curve = s.curve
        }
        ;
        serializedSplines.Add(serializedSpline);
    }
    public void OnAfterDeserialize()
    {
        SoftReset();
        foreach (SerializableNode n in serializedNodes)
        {
            ReadNodesFromSerializedNodes(n);
        }
        foreach (SerializableSpline s in serializedSplines)
        {
            ReadSplinesFromSerializedSplines(s);
        }
        foreach (Node n in nodes)
        {
            AppendSplinesToNodes(n);
        }
    }

    public void ReadNodesFromSerializedNodes(SerializableNode node)
    {
        Node newNode = new Node(node.id, node.location)
        {
            type = node.type,
            waitTime = node.waitTime,
            ControlMode = node.ControlMode,
            boundDirect = node.boundDirect,
            compassDirection = node.compassDirection,
            InControl = node.inControl,
            OutControl = node.outControl,
            inLinks = new List<Spline>(),
            outLinks = new List<Spline>(),
        }
        ;
        nodes.Add(newNode);
    }
    void ReadSplinesFromSerializedSplines(SerializableSpline spline)
    {
        Node anode = nodes.Find(n => n.id == spline.inNodeId);
        Node bnode = nodes.Find(n => n.id == spline.outNodeId);
        Spline newSpline = new Spline(anode, bnode)
        {
            isBezier = spline.isBezier,
            curve = spline.curve
        };
        splines.Add(newSpline);
    }

    public List<int> Duplicate(List<Node> dupeNodes)
    {
        int startIndex = nodes.Count;
        Dictionary<int, int> idConversion = new Dictionary<int, int>();
        List<Node> newNodes = new List<Node>();
        foreach(Node node in dupeNodes)
        {
            Node newNode = MakeNode(GetId(), node.Location);
            newNode.type = node.type;
            newNode.waitTime = node.waitTime;
            newNode.ControlMode = node.ControlMode;
            newNode.boundDirect = node.boundDirect;
            newNode.compassDirection = node.compassDirection;
            newNode.InControl = node.InControl;
            newNode.OutControl = node.OutControl;
            newNodes.Add(newNode);
            idConversion.Add(node.id, newNode.id);
        }
        int endIndex = nodes.Count - 1;
        List<int> newIndexes = new List<int>();
        for(int i=startIndex; i <= endIndex; i++)
        {
            newIndexes.Add(i);
        }

        foreach(Node node in dupeNodes)
        {
            List<Spline> tempList = new List<Spline>();
            tempList.AddRange(node.inLinks);
            foreach (Spline spline in tempList)
            {
                if(idConversion.ContainsKey(spline.inNode.id))
                {
                    Spline newLink = MakeLink(newNodes.Find(n => n.id == idConversion[spline.inNode.id]),
                                              newNodes.Find(n => n.id == idConversion[node.id]));
                    newLink.isBezier = spline.isBezier;
                }
            }
            tempList.Clear();
            tempList.AddRange(node.outLinks);
            foreach (Spline spline in tempList)
            {
                if (idConversion.ContainsKey(spline.outNode.id))
                {
                    Spline newLink = MakeLink(newNodes.Find(n => n.id == idConversion[node.id]), 
                                              newNodes.Find(n => n.id == idConversion[spline.outNode.id]));
                    newLink.isBezier = spline.isBezier;
                }
            }
        }
        return newIndexes;
    }

    public void AppendSplinesToNodes(Node node)
    {
        node.inLinks = splines.FindAll(s => s.outNode.id == node.id);
        node.outLinks = splines.FindAll(s => s.inNode.id == node.id);
    }

    public void SoftReset()
    {
        splines = new List<Spline>();
        nodes = new List<Node>();
    }

    public void Reset()
    {
        SoftReset();
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

    public void MergeNodes(Node nodea, Node nodeb)
    {
        nodea.MoveLinksFromNode(nodeb);
        nodes.Remove(nodeb);
    }

    public Spline MakeLink()
    {
        Node anode = MakeNode(GetId(), transform.position + Vector3.forward);
        Node bnode = MakeNode(GetId(), transform.position + 2 * Vector3.forward);
        Spline link = new Spline(anode, bnode);
        anode.outLinks.Add(link);
        bnode.inLinks.Add(link);
        splines.Add(link);
        return link;
    }

    public Spline MakeLink(Node anode, Node bnode)
    {
        Spline target = splines.Find(s => s.inNode == anode && s.outNode == bnode);
        if(target!= null)
        {
            Debug.LogError("Link already exists between nodes");
            return target;
        }
        Spline link = new Spline(anode, bnode);
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

    public void DeleteLink(Node anode, Node bnode)
    {
        Spline target = splines.Find(s => s.inNode == anode && s.outNode == bnode);
        if (target != null)
            DeleteLink(target);
        else
            Debug.Log("No Link to Delete");
    }

    public void DeleteLink(Spline link)
    {
        link.inNode.outLinks.Remove(link);
        link.outNode.inLinks.Remove(link);
        splines.Remove(link);
    }

    public void DeleteNode(Node target)
    {
        foreach (Spline s in target.inLinks)
        {
            DeleteLink(s, true);
        }
        foreach (Spline s in target.outLinks)
        {
            DeleteLink(s, false);
        }
        nodes.Remove(target);
    }

    public void DeleteLink(Spline target, bool isIn)
    {
        if (isIn)
        {
            target.inNode.outLinks.Remove(target);
        }
        else
        {
            target.outNode.inLinks.Remove(target);
        }
        splines.Remove(target);
    }

    public Spline GetLinkFromNodes(Node anode, Node bnode)
    {
        return splines.Find(s => s.inNode == anode && s.outNode == bnode);
    }

    public Vector3 GetWorldPoint(Spline spline, float t)
    {
        return transform.TransformPoint(spline.curve.GetPoint(t));
    }

    public Vector3 GetWorldVelocity(Spline spline, float t)
    {
        return transform.TransformPoint(spline.curve.GetVelocity(t)) - transform.position;
    }
}

[System.Serializable]
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

    public float GetTotalLength()
    {
        return distanceLookup[distanceLookup.Length - 1];
    }

    public Vector3 GetLinearPointFromDist(float d)
    {
        d = Mathf.Clamp(d, 0, GetTotalLength());
        return GetLinearPoint(d);
    }

    public Vector3 GetLinearPointFromFrac(float f)
    {
        f = Mathf.Clamp(f, 0, 1);
        float dist = f * GetTotalLength();
        return GetLinearPoint(dist);
    }

    public Vector3 GetLinearPoint(float dist)
    {
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

    public Spline(Node anode, Node bnode)
    {
        bool isLoop = anode == bnode;
        curve = new BezierSp(isLoop);
        inNode = anode;
        outNode = bnode;
        UpdateStartPoints();
        UpdateEndPoints();
    }

    public void FlipDirection()
    {
        inNode.outLinks.Remove(this);
        outNode.inLinks.Remove(this);
        Node temp = outNode;
        outNode = inNode;
        inNode = temp;
        inNode.outLinks.Add(this);
        outNode.inLinks.Add(this);
    }

    public void ChangeInNode(Node newNode)
    {
        inNode = newNode;
        newNode.outLinks.Add(this);
        UpdateStartPoints();
    }

    public void ChangeOutNode(Node newNode)
    {
        outNode = newNode;
        newNode.inLinks.Add(this);
        UpdateEndPoints();
    }

    public void UpdateStartPoints()
    {
        curve.SetControlPoint(0, inNode.Location);
        curve.SetControlPoint(1, inNode.OutControl);
    }
    public void UpdateEndPoints()
    {
        curve.SetControlPoint(curve.ControlPointCount - 1, outNode.Location);
        curve.SetControlPoint(curve.ControlPointCount - 2, outNode.InControl);
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
    public float waitTime;
    public BezierControlPointMode ControlMode;
    public NodeDirection boundDirect;
    public GridDirection compassDirection;
    public List<Spline> inLinks;
    public List<Spline> outLinks;
    private Vector3 location;
    private Vector3 inControl;
    private Vector3 outControl;

    public Node(int Id, Vector3 Position)
    {
        id = Id;
        location = Position;
        ControlMode = BezierControlPointMode.Aligned;
        inLinks = new List<Spline>();
        outLinks = new List<Spline>();
        inControl = Position + Vector3.back;
        outControl = Position + Vector3.forward;
    }

    public void MoveLinksFromNode(Node oldNode)
    {
        foreach(Spline spline in oldNode.inLinks)
        {
            spline.ChangeOutNode(this);
        }
        foreach(Spline spline in oldNode.outLinks)
        {
            spline.ChangeInNode(this);
        }
    }

    public Vector3 Location
    {
        get { return location; }
        set
        {
            Vector3 delta = value - location;
            location = value;
            inControl += delta;
            outControl += delta;
            UpdateIn();
            UpdateOut();
        }
    }

    public Vector3 InControl
    {
        get { return inControl; }
        set
        {
            inControl = value;
            UpdateIn();
            Enforce(true);
        }
    }

    public Vector3 OutControl
    {
        get { return outControl; }
        set
        {
            outControl = value;
            UpdateOut();
            Enforce(false);
        }
    }

    public void Enforce(bool isIn)
    {
        if (ControlMode == BezierControlPointMode.Free)
        {
            return;
        }
        Vector3 middle = location;
        Vector3 fixedPoint = isIn ? InControl : OutControl;
        Vector3 enforcedPoint = isIn ? OutControl : InControl;
        Vector3 enforcedTangent = middle - fixedPoint;
        if (ControlMode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, enforcedPoint);
        }
        if (isIn)
        {
            outControl = middle + enforcedTangent;
            UpdateOut();
        }
        else
        {
            inControl = middle + enforcedTangent;
            UpdateIn();
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
            link.UpdateStartPoints();
        }
    }
}

public enum NodeDirection
{
    In,
    Out,
    None
}

public static class NodeDirectionExtensions
{
    public static NodeDirection Opposite(this NodeDirection direction)
    {
        if (direction == NodeDirection.In || direction == NodeDirection.Out)
            return direction == NodeDirection.In ? NodeDirection.Out : NodeDirection.In;
        else
            return NodeDirection.None;
    }
}