using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

    private int selectedIndex = -1;
    private int selectedIndex2 = -1;

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
        DrawBetweenSelections();
    }

    public void DrawBetweenSelections()
    {
        Handles.color = Color.red;
        Handles.DrawLine(network.nodes[selectedIndex].Location, network.nodes[selectedIndex2].Location);
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

    public override void OnInspectorGUI()
    {
        network = target as Network;
        EditorGUILayout.LabelField(string.Format("Nodes: {0}", network.nodes.Count));
        EditorGUILayout.LabelField(string.Format("Links: {0}", network.splines.Count));
        showLabels = EditorGUILayout.ToggleLeft("Show Labels", showLabels);
        selectedIndex = Mathf.Clamp(EditorGUILayout.IntField("Selected Node", selectedIndex), 0, network.nodes.Count - 1);
        if (selectedIndex >= 0 && selectedIndex < network.nodes.Count)
        {
            DrawSelectedPointInspector();
            if (GUILayout.Button("Add Link From"))
            {
                Undo.RecordObject(network, "Add link from");
                network.MakeLink(network.nodes[selectedIndex], true, network.nodes[selectedIndex].Location + Vector3.left);
                selectedIndex = network.nodes.Count - 1;
                EditorUtility.SetDirty(network);
            }
            if (GUILayout.Button("Add Link To"))
            {
                Undo.RecordObject(network, "Add link to");
                network.MakeLink(network.nodes[selectedIndex], false, network.nodes[selectedIndex].Location + Vector3.right);
                selectedIndex = network.nodes.Count - 1;
                EditorUtility.SetDirty(network);
            }
            if(GUILayout.Button("Add Link Between"))
            {
                Undo.RecordObject(network, "Add link between");
                network.MakeLink(network.nodes[selectedIndex], network.nodes[selectedIndex2]);
                selectedIndex = network.nodes.Count - 1;
                EditorUtility.SetDirty(network);
            }
            selectedIndex2 = Mathf.Clamp(EditorGUILayout.IntField("Second Node", selectedIndex2), 0, network.nodes.Count - 1);
        }
        if (GUILayout.Button("Add Initial Link"))
        {
            Undo.RecordObject(network, "Add initial link");
            network.MakeLink();
            EditorUtility.SetDirty(network);
        }
    }

    private void DrawSelectedPointInspector()
    {
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Location", network.nodes[selectedIndex].Location);
        Vector3 point2 = EditorGUILayout.Vector3Field("ControlIn", network.nodes[selectedIndex].InControl);
        Vector3 point3 = EditorGUILayout.Vector3Field("ControlOut", network.nodes[selectedIndex].OutControl);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(network, "Move Node");
            EditorUtility.SetDirty(network);
            network.nodes[selectedIndex].Location = point;
            network.nodes[selectedIndex].InControl = point2;
            network.nodes[selectedIndex].OutControl = point3;
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)
        EditorGUILayout.EnumPopup("Mode", network.nodes[selectedIndex].ControlMode);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(network, "Change Control Mode");
            network.nodes[selectedIndex].ControlMode = mode;
            EditorUtility.SetDirty(network);
        }
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
            //float size = HandleUtility.GetHandleSize(point);
            //Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap);
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

        Handles.color = modeColors[(int)node.ControlMode];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = network.nodes.IndexOf(node);
            Repaint();
        }
        if (selectedIndex == network.nodes.IndexOf(node))
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(network, "Move Node");
                EditorUtility.SetDirty(network);
                network.nodes[selectedIndex].Location = handleTransform.InverseTransformPoint(point);
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
            selectedIndex = network.nodes.IndexOf(node);
            Repaint();
        }
        if (selectedIndex == network.nodes.IndexOf(node))
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(network, "Move in control");
                EditorUtility.SetDirty(network);
                network.nodes[selectedIndex].InControl = handleTransform.InverseTransformPoint(point);
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
            selectedIndex = network.nodes.IndexOf(node);
            Repaint();
        }
        if (selectedIndex == network.nodes.IndexOf(node))
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(network, "Move out control");
                EditorUtility.SetDirty(network);
                network.nodes[selectedIndex].OutControl = handleTransform.InverseTransformPoint(point);
            }
        }
        if (showLabels)
        {
            Handles.Label(point, point.ToString() +
                "\nCOut: " + network.nodes.IndexOf(node));
        }
    }
}
