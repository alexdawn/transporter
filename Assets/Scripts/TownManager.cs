using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownManager : MonoBehaviour {
    SquareCell townCentre;
    List<SquareCell> townRoads = new List<SquareCell>(), townBuildings = new List<SquareCell>();
    string townName;
    string[] randomNames = { "chester", "ingham", "ham", "ford", "don", "den", "bridge", "ton"};
    string[] randomNamesPrefix = { "Exe", "Axe", "Bir", "Stan", "Lon", "Far", "Man", "Wo", "Bri"};

    public void Init(SquareCell start)
    {
        townCentre = start;
        townName = randomNamesPrefix[Random.Range(0, randomNamesPrefix.Length)] + randomNames[Random.Range(0, randomNames.Length)];
        TextMesh label = this.gameObject.GetComponentInChildren<TextMesh>();
        label.text = townName;
        townRoads.Add(start);
        for(int i=0; i<10;i++)
        {
            AddRoad();
        }
        for(int b=0; b<15;b++)
        {
            MakeBuilding();
        }
    }

    public void AddRoad()
    {
        SquareCell randomRoad = townRoads[Random.Range(0, townRoads.Count)];
        GridDirection randomDirection = (GridDirection)(Random.Range(0, 3) * 2);
        SquareCell randomNeighbour = randomRoad.GetNeighbor(randomDirection);
        if(randomNeighbour && randomNeighbour.UrbanLevel == 0 && !townRoads.Contains(randomNeighbour))
        {
            randomRoad.AddRoad(randomDirection);
            randomNeighbour.AddRoad(randomDirection.Opposite());
            townRoads.Add(randomNeighbour);
        }
    }

    public void MakeBuilding()
    {
        SquareCell randomRoad = townRoads[Random.Range(0, townRoads.Count)];
        GridDirection randomDirection = (GridDirection)(Random.Range(0, 3) * 2);
        SquareCell randomNeighbour = randomRoad.GetNeighbor(randomDirection);
        if (randomNeighbour && !randomNeighbour.HasRoads)
        {
            randomNeighbour.UrbanLevel++;
            if (!townBuildings.Contains(randomNeighbour))
            {
                townBuildings.Add(randomNeighbour);
            }
        }
    }
}
