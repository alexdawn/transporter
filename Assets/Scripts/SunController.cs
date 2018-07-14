using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunController : MonoBehaviour {
    public float seconds_per_day_night_cycle;
    private Transform tranform;
    private Light light;
    public float angle;
    public string time;
    public Text clockDisplay;
    // Use this for initialization
    void Start () {
        tranform = gameObject.GetComponent<Transform>();
        light = gameObject.GetComponent<Light>();
    }

    string AngleToTime(float angle)
    {
        float decimalTime = (angle * 24f / 360f + 6f) % 24f;
        float hours = Mathf.Floor(decimalTime);
        float minutes = Mathf.Floor((decimalTime % 1) * 60);
        return hours.ToString("00") + ":" + minutes.ToString("00");
    }
	
	// Update is called once per frame
	void Update () {
        angle += ((360f / (seconds_per_day_night_cycle)) * Time.deltaTime);
        angle = angle % 360f;
        transform.eulerAngles = new Vector3(angle, -90f, 0);
        time = AngleToTime(angle);
        clockDisplay.text = time;
        if (angle > 180.5f || angle < -0.5f)
        {
            light.intensity = 0f;
        }
        else if(angle > 179.5f) // blend light intensity at sunset
        {
            light.intensity = 1f - (angle - 179.5f);
        }
        else if(angle < 0.5f) // blend light intensity at dawn
        {
            light.intensity = 1f + (angle - 0.5f);
        }
        else
        {
            light.intensity = 1f;
        }
    }
}
