using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Follower : MonoBehaviour {

    public int segments;
    public int[] segmentTemplateIndex;
    public float[] segment_lengths;
    public GameObject[] templates;

    [SerializeField]
    private Vector3[] positions;
    private GameObject[] prefabs;
    // Use this for initialization
    void Start() {
        Array.Resize(ref positions, segments + 1);
        Array.Resize(ref prefabs, segments);
        for (int i=0; i<=segments; i++)
        {
            positions[i] = new Vector3();
            if(i!= segments)
            {
                prefabs[i] = Instantiate(templates[segmentTemplateIndex[i]], this.transform);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        positions[0] = this.transform.position;
        for(int i=1; i<positions.Length; i++)
        {
            DragSegment(i, positions[i-1]);
        }
    }

    void DragSegment(int i, Vector3 input)
    {
        Vector3 delta = input - positions[i];
        float angle = Mathf.Atan2(delta.z, delta.x);
        positions[i].x = input.x - Mathf.Cos(angle) * segment_lengths[i-1];
        positions[i].z = input.z - Mathf.Sin(angle) * segment_lengths[i-1];
        positions[i].y = input.y;
        DrawSegment(i);
    }

    void DrawSegment(int i)
    {
        prefabs[i-1].transform.position = Vector3.Lerp(positions[i-1], positions[i], 0.5f);
        prefabs[i-1].transform.LookAt(positions[i-1]);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Gizmos.DrawLine(positions[i], positions[i + 1]);
        }
    }
}
