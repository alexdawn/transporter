using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {
    TextMesh townLabel;
    // Use this for initialization
    void Start () {
        townLabel = this.gameObject.GetComponentInChildren<TextMesh>();
    }
	
	// Update is called once per frame
	void Update () {
        townLabel.transform.LookAt(Camera.main.transform);
    }
}
