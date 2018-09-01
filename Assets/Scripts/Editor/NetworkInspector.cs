using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[CustomEditor(typeof(Network))]
public class NetworkInspector : Editor {

    private Network network;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private const int stepsPerCurve = 10;
    private const float directionScale = 0.02f;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    private bool showLabels;

    private List<int> selectedIndices = new List<int>();

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private void OnSceneGUI()
    {
        network = target as Network;
        handleTransform = network.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
        handleTransform.rotation : Quaternion.identity;

        DrawNodes(network.nodes);
        DrawLines(network.splines);
        if(selectedIndices.Count==2)
            DrawBetweenSelections();
    }

    public void DrawBetweenSelections()
    {
        Handles.color = Color.red;
        Handles.DrawLine(handleTransform.TransformPoint(network.nodes[selectedIndices[0]].Location),
                         handleTransform.TransformPoint(network.nodes[selectedIndices[1]].Location));
    }

    public void DrawNodes(List<Node> nodes)
    {
        foreach(Node n in nodes)
        {
            ShowNode(n);
            Handles.color = Color.grey;
            // lines from node to control handles
            Handles.DrawLine(handleTransform.TransformPoint(n.Location), handleTransform.TransformPoint(n.InControl));
            Handles.DrawLine(handleTransform.TransformPoint(n.Location), handleTransform.TransformPoint(n.OutControl));
        }
    }

    public void DrawLines(List<Spline> splines)
    {
        foreach (Spline s in splines)
        {
            Handles.DrawBezier(handleTransform.TransformPoint(s.inNode.Location),
                handleTransform.TransformPoint(s.outNode.Location),
                handleTransform.TransformPoint(s.inNode.OutControl),
                handleTransform.TransformPoint(s.outNode.InControl), 
                Color.white, null, 2f);
            ShowDirections(s);
        }
    }

    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

    public override void OnInspectorGUI()
    {
        network = target as Network;
        EditorGUILayout.LabelField(string.Format("Nodes: {0}", network.nodes.Count));
        EditorGUILayout.LabelField(string.Format("Links: {0}", network.splines.Count));
        showLabels = EditorGUILayout.ToggleLeft("Show Labels", showLabels);
        if (selectedIndices.Count == 1)
        {
            EditorGUILayout.BeginHorizontal();
            selectedIndices[0] = Mathf.Clamp(EditorGUILayout.IntField("Selected Node", selectedIndices[0]), 0, network.nodes.Count - 1);
            if (GUILayout.Button("<", miniButtonWidth))
            {
                selectedIndices[0] = Mathf.Clamp(--selectedIndices[0], 0, network.nodes.Count - 1);
            }
            if (GUILayout.Button(">", miniButtonWidth))
            {
                selectedIndices[0] = Mathf.Clamp(++selectedIndices[0], 0, network.nodes.Count - 1);
            }
            EditorGUILayout.EndHorizontal();
            DrawSelectedPointInspector();
            if (GUILayout.Button("Add Link From"))
            {
                Undo.RecordObject(network, "Add link from");
                network.MakeLink(network.nodes[selectedIndices[0]], true, network.nodes[selectedIndices[0]].Location + Vector3.left);
                selectedIndices[0] = network.nodes.Count - 1;
                //EditorUtility.SetDirty(network);
            }
            if (GUILayout.Button("Add Link To"))
            {
                Undo.RecordObject(network, "Add link to");
                network.MakeLink(network.nodes[selectedIndices[0]], false, network.nodes[selectedIndices[0]].Location + Vector3.right);
                selectedIndices[0] = network.nodes.Count - 1;
                //EditorUtility.SetDirty(network);
            }
        }
        else
        {
            EditorGUILayout.LabelField(string.Format("Nodes: {0}", string.Join(";", selectedIndices.Select(x => x.ToString()).ToArray())));
        }
        if(GUILayout.Button("Delete Node(s)"))
        {
            List<Node> marked = new List<Node>();
            foreach(int i in selectedIndices)
            {
                marked.Add(network.nodes[i]);
            }
            foreach(Node n in marked)
            {
                network.DeleteNode(n);
            }
            selectedIndices.Clear();
        }
        if (selectedIndices.Count == 2)
        {
            if(GUILayout.Button("Add Link Between"))
            {
                Undo.RecordObject(network, "Add link between");
                network.MakeLink(network.nodes[selectedIndices[0]], network.nodes[selectedIndices[1]]);
                //EditorUtility.SetDirty(network);
            }
        }
        if (GUILayout.Button("Add Disconnected Link"))
        {
            Undo.RecordObject(network, "Add initial link");
            network.MakeLink();
            selectedIndices[0] = network.nodes.Count - 1;
            //EditorUtility.SetDirty(network);
        }
    }

    private void DrawSelectedPointInspector()
    {
        int lastPoint = selectedIndices[selectedIndices.Count - 1];
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Location", network.nodes[lastPoint].Location);
        Vector3 point2 = EditorGUILayout.Vector3Field("ControlIn", network.nodes[lastPoint].InControl);
        Vector3 point3 = EditorGUILayout.Vector3Field("ControlOut", network.nodes[lastPoint].OutControl);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(network, "Move Node");
            //EditorUtility.SetDirty(network);
            Vector3 delta = point - network.nodes[lastPoint].Location;
            network.nodes[lastPoint].Location = point;
            Vector3 delta2 = point2 - network.nodes[lastPoint].InControl;
            network.nodes[lastPoint].InControl = point2;
            Vector3 delta3 = point3 - network.nodes[lastPoint].OutControl;
            network.nodes[lastPoint].OutControl = point3;
            // offset other selections equally
            for(int i=0; i < selectedIndices.Count-1; i++)
            {
                network.nodes[lastPoint].Location+=delta;
                network.nodes[lastPoint].InControl+=delta2;
                network.nodes[lastPoint].OutControl+=delta3;
            }
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)
        // todo change all selected modes
        EditorGUILayout.EnumPopup("Mode", network.nodes[lastPoint].ControlMode);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(network, "Change Control Mode");
            network.nodes[lastPoint].ControlMode = mode;
            EditorUtility.SetDirty(network);
        }
        EditorGUI.BeginChangeCheck();
        NodeType type = (NodeType)
        EditorGUILayout.EnumPopup("Node Type", network.nodes[lastPoint].type);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(network, "Change Node Type");
            network.nodes[lastPoint].type = type;
            EditorUtility.SetDirty(network);
        }
        network.nodes[lastPoint].waitTime = EditorGUILayout.FloatField("Wait Time", network.nodes[lastPoint].waitTime);
    }

    private void ShowDirections(Spline spline)
    {
        Handles.color = Color.green;
        Vector3 point = network.GetWorldPoint(spline, 0f);
        Handles.DrawLine(point, point + network.GetWorldVelocity(spline, 0f) * directionScale);
        int steps = stepsPerCurve * spline.curve.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = network.GetWorldPoint(spline, i / (float)steps);
            Handles.DrawLine(point, point + network.GetWorldVelocity(spline, i / (float)steps) * directionScale);
        }
    }

    private void ShowNode(Node node)
    {
        ShowPoint(node);
        ShowInControl(node);
        ShowOutControl(node);
    }

    private void ShowPoint(Node node)
    {
        Vector3 point = handleTransform.TransformPoint(node.Location);
        float size = HandleUtility.GetHandleSize(point);
        if(selectedIndices.Contains(network.nodes.IndexOf(node))){
            Handles.color = Color.red;
            Handles.Button(point, handleRotation, size * handleSize * 2f, size * pickSize * 2f, Handles.DotHandleCap);
        }
        Handles.color = modeColors[(int)node.ControlMode];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            if (Event.current.shift && !selectedIndices.Contains(network.nodes.IndexOf(node)))
            {
                selectedIndices.Add(network.nodes.IndexOf(node));
            }
            else
            {
                selectedIndices.Clear();
                selectedIndices.Add(network.nodes.IndexOf(node));
            }
            Repaint();
        }
        if (selectedIndices.Count > 0 && selectedIndices[selectedIndices.Count-1] == network.nodes.IndexOf(node))
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(network, "Move Node");
                EditorUtility.SetDirty(network);
                Vector3 delta = handleTransform.InverseTransformPoint(point) - network.nodes[selectedIndices[selectedIndices.Count - 1]].Location;
                foreach(int i in selectedIndices)
                {
                    network.nodes[i].Location+=delta;
                }
            }
        }
        if (showLabels)
        {
            Handles.Label(point, point.ToString() +
            "\nPoint: " + network.nodes.IndexOf(node) +
            "\nMode: " + node.ControlMode);
        }
    }

    private void ShowInControl(Node node)
    {
        Vector3 point = handleTransform.TransformPoint(node.InControl);
        float size = HandleUtility.GetHandleSize(point);

        Handles.color = modeColors[(int)node.ControlMode];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            if (Event.current.shift && !selectedIndices.Contains(network.nodes.IndexOf(node)))
            {
                selectedIndices.Add(network.nodes.IndexOf(node));
            }
            else
            {
                selectedIndices.Clear();
                selectedIndices.Add(network.nodes.IndexOf(node));
            }
            Repaint();
        }
        if (selectedIndices.Count > 0 && selectedIndices[selectedIndices.Count - 1] == network.nodes.IndexOf(node))
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(network, "Move in control");
                //EditorUtility.SetDirty(network);
                network.nodes[selectedIndices[selectedIndices.Count - 1]].InControl = handleTransform.InverseTransformPoint(point);
            }
        }
        if (showLabels)
        {
            Handles.Label(point, point.ToString() +
            "\nCIn: " + network.nodes.IndexOf(node));
        }
    }

    private void ShowOutControl(Node node)
    {
        Vector3 point = handleTransform.TransformPoint(node.OutControl);
        float size = HandleUtility.GetHandleSize(point);

        Handles.color = modeColors[(int)node.ControlMode];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            if (Event.current.shift && !selectedIndices.Contains(network.nodes.IndexOf(node)))
            {
                selectedIndices.Add(network.nodes.IndexOf(node));
            }
            else
            {
                selectedIndices.Clear();
                selectedIndices.Add(network.nodes.IndexOf(node));
            }
            Repaint();
        }
        if (selectedIndices.Count > 0 && selectedIndices[selectedIndices.Count - 1] == network.nodes.IndexOf(node))
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(network, "Move out control");
                //EditorUtility.SetDirty(network);
                network.nodes[selectedIndices[selectedIndices.Count - 1]].OutControl = handleTransform.InverseTransformPoint(point);
            }
        }
        if (showLabels)
        {
            Handles.Label(point, point.ToString() +
                "\nCOut: " + network.nodes.IndexOf(node));
        }
    }
}
