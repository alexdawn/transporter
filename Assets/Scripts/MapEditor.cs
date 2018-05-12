using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapEditor : MonoBehaviour {
    public Color[] colors;
    public SquareGrid squareGrid;
    private Color activteColor;


    void Awake()
    {
        SelectColor(0);
    }


    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }


    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            squareGrid.ColorCell(hit.point, activteColor);
        }
    }


    public void SelectColor (int index)
    {
        Debug.Log("selects " + index);
        activteColor = colors[index];
    }
}
