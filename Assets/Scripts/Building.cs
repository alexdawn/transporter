using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Building: ScriptableObject
{
    public string buildingName;
    public int width;
    public int depth;
    public GridDirection frontageDirection;
    public int startYear;
    public int endYear;
    public Color color;
    public Transform prefabBuilding;
    public GroundMaterial groundMaterial;
    public bool flatFoundations;
    public TownManager owner;
    private SquareCell[] foundations;
    private int height; // height of the origin cell

    public void SetupFoundations(SquareCell origin)
    {
        List<SquareCell> foundationList = new List<SquareCell>();
        height = origin.GetMaxElevation();
        owner = origin.Town;
        SquareCell tempCellx = origin;
        SquareCell tempCelly = origin;
        for (int x=0;x<width;x++)
        {
            tempCelly = tempCellx;
            for (int y=0;y< depth; y++)
            {
                foundationList.Add(tempCelly);
                tempCelly = tempCelly.GetNeighbor(GridDirection.N);
            }
            tempCellx = tempCellx.GetNeighbor(GridDirection.E);
        }
        Foundations = foundationList.ToArray();
    }

    public Building GetClone
    {
        get { return (Building)this.MemberwiseClone(); }
    }

    SquareCell[] Foundations
    {
        get { return foundations; }
        set {
            foundations = value;
            Building build = Instantiate(this);
            foreach (SquareCell foundation in foundations)
            {
                if (foundation.BuildingOnSquare)
                {
                    Destroy(foundation.BuildingOnSquare);
                }
                foundation.BuildingOnSquare = build;
                if (flatFoundations)
                {
                    foundation.FlattenTo(height);
                }
                if (groundMaterial != null)
                {
                    foundation.Tile = groundMaterial;
                }
                //GroundMaterial.SetToMud(ref foundation.Tile);
            }
        }
    }
}
