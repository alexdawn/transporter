using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_Show : MonoBehaviour
{
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Toggle()
    {
        bool state = this.gameObject.activeSelf;
        this.gameObject.SetActive(!state);
    }
}

