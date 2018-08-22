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
    private const float directionScale = 0.1f;

    private void OnSceneGUI()
    {

        network = target as Network;
        handleTransform = network.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
        handleTransform.rotation : Quaternion.identity;

        DrawNodes(network.nodes);
        DrawLines(network.splines);
    }

    public void DrawNodes(List<Node> nodes)
    {
        foreach(Node n in nodes)
        {
            Handles.color = Color.red;

            ShowPoint(n, n.InControl);
            ShowPoint(n, n.Location);
            ShowPoint(n, n.OutControl);

            Handles.DrawLine(n.Location, n.InControl);
            Handles.DrawLine(n.Location, n.OutControl);
        }
    }

    public void DrawLines(List<Spline> splines)
    {
        foreach (Spline s in splines)
        {
            Handles.DrawBezier(s.inNode.Location, s.outNode.Location, s.inNode.OutControl, s.outNode.InControl, Color.white, null, 2f);
            ShowDirections(s);
        }
    }

    public override void OnInspectorGUI()
    {
        network = target as Network;
        EditorGUI.BeginChangeCheck();
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
        GUILayout.Label("Selected Node");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Location", network.nodes[selectedIndex].Location);
        Vector3 point2 = EditorGUILayout.Vector3Field("ControlIn", network.nodes[selectedIndex].InControl);
        Vector3 point3 = EditorGUILayout.Vector3Field("ControlOut", network.nodes[selectedIndex].OutControl);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(network, "Move Node");
            EditorUtility.SetDirty(network);
            network.nodes[selectedIndex].Location = point;
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
        Vector3 point = spline.curve.GetPoint(0f);
        Handles.DrawLine(point, point + spline.curve.GetDirection(0f) * directionScale);
        int steps = stepsPerCurve * spline.curve.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = spline.curve.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.curve.GetDirection(i / (float)steps) * directionScale);
        }
    }

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private int selectedIndex = -1;

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private Vector3 ShowPoint(Node node, Vector3 location)
    {
        Vector3 point = handleTransform.TransformPoint(location);
        float size = HandleUtility.GetHandleSize(point);
        //if (index == 0)
        //{
        //    size *= 2f;
        //}
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
        Handles.Label(point, point.ToString() + 
            "\nPoint: " + network.nodes.IndexOf(node) +
            "\nMode: " + node.ControlMode);
        return point;
    }
}
