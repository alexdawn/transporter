using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunController : MonoBehaviour {
    public float seconds_per_day_night_cycle;
    public float seconds_per_day;
    private Transform tranform;
    private Light light;
    public float angle;
    public string time;
    public float date = 0;
    public Text clockDisplay;
    public Material windows;
    // Use this for initialization
    void Start () {
        tranform = gameObject.GetComponent<Transform>();
        transform.eulerAngles = new Vector3(angle, -90f, 0);
        light = gameObject.GetComponent<Light>();
    }

    string AngleToTime(float angle)
    {
        float decimalTime = (angle * 24f / 360f + 6f) % 24f;
        float hours = Mathf.Floor(decimalTime);
        float minutes = Mathf.Floor((decimalTime % 1) * 60);
        return hours.ToString("00") + ":" + minutes.ToString("00");
    }

    string DayToDate(float date)
    {
        int day = (int)Mathf.Floor(date % 30) + 1;
        int month = (int)Mathf.Floor(Mathf.Floor(date / 30) % (12)) + 1;
        int year = 1900 + (int)Mathf.Floor(Mathf.Floor(date / (30 * 12)) % (30 * 12));
        return day.ToString("00") + "/" + month.ToString("00") + "/" + year.ToString("0000");
    }
	
	// Update is called once per frame
	void Update () {
        angle += ((360f / (seconds_per_day_night_cycle)) * Time.deltaTime);
        date += (1 / seconds_per_day) * Time.deltaTime;
        angle = angle % 360f;
        transform.eulerAngles = new Vector3(angle, -90f, 0);
        time = AngleToTime(angle);
        clockDisplay.text = DayToDate(date) + " " + time;
        if (angle > 180.5f || angle < -0.5f)
        {
            RenderSettings.ambientIntensity = 0.25f;
            light.intensity = 0f;
            windows.EnableKeyword("_EMISSION");
        }
        else if(angle > 179.5f) // blend light intensity at sunset
        {
            float light_strength = 1f - (angle - 179.5f);
            RenderSettings.ambientIntensity = Mathf.Max(0.25f, light_strength);
            light.intensity = light_strength;
        }
        else if(angle < 0.5f) // blend light intensity at dawn
        {
            float light_strength = 1f + (angle - 0.5f);
            RenderSettings.ambientIntensity = Mathf.Max(0.25f, light_strength);
            light.intensity = light_strength;
        }
        else
        {
            RenderSettings.ambientIntensity = 1f;
            light.intensity = 1f;
            windows.DisableKeyword("_EMISSION");
        }
    }
}
