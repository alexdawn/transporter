using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour {
    private Transform tranform;
    private Vector3 angles;
	// Use this for initialization
	void Start () {
        tranform = gameObject.GetComponent<Transform>();
        angles = new Vector3(90f, 0f, 0f);

    }
	
	// Update is called once per frame
	void Update () {
        angles.x = (360f/(60f)) * Time.deltaTime;
        tranform.Rotate(angles);
    }
}
