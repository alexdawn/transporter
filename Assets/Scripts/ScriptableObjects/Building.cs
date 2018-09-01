using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    house,
    flats,
    offices,
    leisure,
    landmark,
    hotel,
    shops,
    mixed,
    industrial
}

[CreateAssetMenu]
public class Building: ScriptableObject
{
    public string buildingName;
    public string description;
    public int width;
    public int depth;
    public GridDirection frontageDirection;
    public int startYear;
    public int endYear;
    public bool randomRotation;
    public bool uniqueInTown;
    public bool hasColorVariants;
    public int variantMaterialId;
    public BuildingColorPallet pallet;
    public Transform prefabBuilding;
    public GroundMaterial groundMaterial;
    public BuildingType buildingType;
    public int passengerCreation;
    public int population;
    public int jobs;
    public int mailCreation;
    public bool acceptsPassengers;
    public bool acceptsMail;
    public bool acceptsGoods;
    public bool flatFoundations;
    private TownManager owner;
    private SquareCell[] foundations;
    private int height; // height of the origin cell
    private Transform buildingModel;

    public void Construct(SquareCell origin, Vector3 position, GridHash hash)
    {
        SetupFoundations(origin);
        CreatePrefab(position, hash);
    }

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

    public Transform BuildingModel
    {
        get { return buildingModel; }
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
            Building build = this;
            for(int i=0;i<foundations.Length;i++)
            {
                if (foundations[i].BuildingOnSquare != build && foundations[i].BuildingOnSquare != null)
                {
                    foundations[i].BuildingOnSquare.Demolish();
                }
                if(foundations[i].BuildingOnSquare != build)
                {
                    foundations[i].BuildingOnSquare = build;
                }
                if (flatFoundations)
                {
                    foundations[i].FlattenTo(height);
                }
                if (groundMaterial != null)
                {
                    foundations[i].Tile = Instantiate(groundMaterial);
                }
                GroundMaterial tempPass = foundations[i].Tile;
                GroundMaterial.SetToMud(foundations[i], ref tempPass);
                foundations[i].Refresh();
            }
        }
    }

    public void Demolish()
    {
        for(int i=0;i< foundations.Length; i++)
        {
            foundations[i].BuildingOnSquare = null;
            GroundMaterial tempPass = foundations[i].Tile;
            GroundMaterial.SetToMud(foundations[i], ref tempPass);
        }
        Destroy(this);
    }

    private void OnDestroy()
    {
        if(buildingModel != null)
        {
            Destroy(buildingModel.gameObject);
        }
    }

    public void CreatePrefab(Vector3 position, GridHash hash)
    {
        position = GridCoordinates.ToPosition(foundations[0].coordinates) + Vector3.up * foundations[0].CentreElevation * GridMetrics.elevationStep;
        buildingModel = Instantiate(prefabBuilding);
        buildingModel.localPosition = position;
        if (randomRotation)
        {
            buildingModel.localRotation = buildingModel.localRotation * Quaternion.Euler(0, 90 * Mathf.Round(hash.b * 4), 0);
        }
        if (hasColorVariants)
        {
            Renderer[] renderers = buildingModel.GetComponentsInChildren<Renderer>();
            Color randomColor = pallet.colorPallet[UnityEngine.Random.Range(0, pallet.colorPallet.Length)];
            foreach (Renderer r in renderers)
            {
                foreach(Material m in r.materials)
                {
                    if(m.name == "Company (Instance)")
                    {
                        m.color = randomColor;
                    }
                }
            }
        }
    }
}
