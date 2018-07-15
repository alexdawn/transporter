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
    public TownManager owner;
    private SquareCell[] foundations;
    private int height; // height of the origin cell

    public void SetupFoundations(SquareCell origin)
    {
        List<SquareCell> foundationList = new List<SquareCell>();
        height = origin.GetMaxElevation();
        owner = origin.Town;
        SquareCell tempCellx = origin;
        SquareCell tempCelly;
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
            foreach (SquareCell foundation in foundations)
            {
                foundation.BuildingOnSquare = this;
                foundation.FlattenTo(height);
                foundation.Tile = groundMaterial;
                foundation.Tile.SetToMud();
            }
        }
    }
}
