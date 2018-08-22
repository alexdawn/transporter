using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Follower : MonoBehaviour {

    public int segments;
    public float segment_length;
    public GameObject template;

    [SerializeField]
    private Vector3[] positions;
    private GameObject[] prefabs;
    // Use this for initialization
    void Start() {
        Array.Resize(ref positions, segments);
        Array.Resize(ref prefabs, segments - 1);
        for (int i=0; i<segments; i++)
        {
            positions[i] = new Vector3();
            if(i!= segments - 1)
            {
                prefabs[i] = Instantiate(template, this.transform);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        positions[0] = this.transform.position;
        for(int i=0; i<positions.Length-1; i++)
        {
            dragSegment(i + 1, positions[i]);
            if (i != 0)
            {
                drawSegment(i);
            }
        }
    }

    void dragSegment(int i, Vector3 input)
    {
        Vector3 delta = input - positions[i];
        float angle = Mathf.Atan2(delta.z, delta.x);
        positions[i].x = input.x - Mathf.Cos(angle) * segment_length;
        positions[i].z = input.z - Mathf.Sin(angle) * segment_length;
        positions[i].y = input.y;
        drawSegment(i);
    }

    void drawSegment(int i)
    {
        prefabs[i - 1].transform.position = Vector3.Lerp(positions[i-1], positions[i], 0.5f);
        prefabs[i - 1].transform.LookAt(positions[i-1]);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Gizmos.DrawLine(positions[i], positions[i + 1]);
        }
    }
}
