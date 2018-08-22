using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TintMaterial : MonoBehaviour {
    public int MaterialIndex;
    public Color CompanyColor;
	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Renderer>().materials[MaterialIndex].color = CompanyColor;
    }
	
	// Update is called once per frame
	void Update () {
        if (Application.isEditor)
        {
            gameObject.GetComponent<Renderer>().materials[MaterialIndex].color = CompanyColor;
        }

    }
}
