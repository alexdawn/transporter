using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour {
    private Transform tranform;
    private Light light;
    private Vector3 deltaAngles;
	// Use this for initialization
	void Start () {
        tranform = gameObject.GetComponent<Transform>();
        light = gameObject.GetComponent<Light>();
        deltaAngles = new Vector3(0f, 0f, 0f);

    }
	
	// Update is called once per frame
	void Update () {
        deltaAngles.x = ((360f / (60f)) * Time.deltaTime) % 360f;
        tranform.Rotate(deltaAngles);
        if(tranform.eulerAngles.x > 180f || tranform.eulerAngles.x < 0f)
        {
            light.intensity = 0f;
        }
        else
        {
            light.intensity = 1f;
        }
    }
}
