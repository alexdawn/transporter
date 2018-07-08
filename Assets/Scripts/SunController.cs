using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour {
    public float seconds_per_day_night_cycle;
    private Transform tranform;
    private Light light;
    private Vector3 deltaAngles;
    public float angle;
    public float time;
	// Use this for initialization
	void Start () {
        tranform = gameObject.GetComponent<Transform>();
        light = gameObject.GetComponent<Light>();
        deltaAngles = new Vector3(0f, 0f, 0f);

    }

    float AngleToTime(float angle)
    {
        return (angle * 24f / 360f + 6f) % 24f;
    }
	
	// Update is called once per frame
	void Update () {
        deltaAngles.x = ((360f / (seconds_per_day_night_cycle)) * Time.deltaTime) % 360f;
        tranform.Rotate(deltaAngles);
        angle = Quaternion.Angle(transform.rotation, Quaternion.identity); // gets angle from rotation (0,0,0)
        time = AngleToTime(angle);
        if (tranform.eulerAngles.x > 180.5f || tranform.eulerAngles.x < -0.5f)
        {
            light.intensity = 0f;
        }
        else if(tranform.eulerAngles.x > 179.5f) // blend light intensity at sunset
        {
            light.intensity = 1f - (tranform.eulerAngles.x - 179.5f);
        }
        else if(tranform.eulerAngles.x < 0.5f) // blend light intensity at dawn
        {
            light.intensity = 1f + (tranform.eulerAngles.x - 0.5f);
        }
        else
        {
            light.intensity = 1f;
        }
    }
}
