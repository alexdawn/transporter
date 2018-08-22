using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathTree))]
public class PathTreeInspector : Editor
{
    private Transform handleTransform;
    private Quaternion handleRotation;
    private PathTree tree;
    private PathLine selection;

    public override void OnInspectorGUI()
    {
        tree = target as PathTree;
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(tree, "Add Segment");
            selection = tree.AddSegment(selection);
            EditorUtility.SetDirty(tree);
        }
        GUI.Label(new Rect(10, 10, 100, 20), "Segments " + tree.Segments.Count.ToString());
        if (selection == null && tree.Segments.Count > 0)
        {
            selection = tree.Segments[0];
        }
        if(selection != null)
        {
            GUI.Label(new Rect(10, 10, 100, 20), "Selection " + selection.ToString());
            if (GUILayout.Button("Select Previous"))
            {
                selection = selection.Sources[0];
            }
            if (GUILayout.Button("Select Next"))
            {
                selection = selection.Destinations[0];
            }
        }
    }
}