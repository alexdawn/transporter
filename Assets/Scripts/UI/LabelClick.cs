using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabelClick : MonoBehaviour
{
    public GameObject townInfo;
    GameObject instance;
    TownManager town;
    Camera townCamera;

    void Start()
    {
        GameObject canvas = GameObject.Find("EditorCanvas");
        instance = Instantiate(townInfo, canvas.transform);
        town = GetComponentInParent<TownManager>();
        townCamera = GetComponentInChildren<Camera>(true);
        if(townCamera == null)
        {
            Debug.Log("could not find camera");
        }
        townCamera.targetTexture = new RenderTexture(256, 256, 16); //RenderTextureFormat.ARGB32
        townCamera.Render();
        instance.transform.Find("Title").GetComponent<Text>().text = town.TownName;
        instance.transform.Find("LabelPopulation").GetComponent<Text>().text = "Population: " + town.Population.ToString("#");
        instance.transform.Find("LabelRoads").GetComponent<Text>().text = "Roads: " + town.Roads.ToString("#");
        instance.transform.Find("RawImage").GetComponent<RawImage>().texture = townCamera.targetTexture;
    }

    void OnMouseDown()
    {
        instance.SetActive(true);
        instance.transform.Find("LabelPopulation").GetComponent<Text>().text = "Population: " + town.Population.ToString("#");
        instance.transform.Find("LabelRoads").GetComponent<Text>().text = "Roads: " + town.Roads.ToString("#");
    }
}
