using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rudimentary AI, essentially just plays
public class AICityPlayer : CityPlayer
{
    private List<Building> buildings = new List<Building>();
    private List<Vector2> eligibleLocations = new List<Vector2>();
    private List<Vector2> visitedLocations = new List<Vector2>();
    private HexTileMapGenerator hexMap;

    public int waitTime = 250; // FixedUpdates between actions, 50=1s by default
    private int timeWaited; // First move is a second after the start

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        hexMap = GameObject.Find("CPTileGen").GetComponent<HexTileMapGenerator>();
        Vector2 startLocation = new Vector2();
        startLocation.x = Random.value * hexMap.mapWidth;
        startLocation.y = Random.value * hexMap.mapHeight;
        HexTile t = hexMap.GetHexTile((int) startLocation.x, (int) startLocation.y);
        while(t.terrain.name.Equals("Water"))
        {
            startLocation.x = Random.value * hexMap.mapWidth;
            startLocation.y = Random.value * hexMap.mapHeight;
            t = hexMap.GetHexTile((int) startLocation.x, (int) startLocation.y);
        }

        Debug.Log(startLocation + ":" + hexMap.mapWidth + ":" + hexMap.mapHeight);
        eligibleLocations.Add(startLocation);
        timeWaited = waitTime - 50;
    }

    new void  Update()
    {
        // Override to stop the mouse stuff.
    }

    // Fixed Update is called 50 times/sec by default, which is the rate of physics calculations.
    void FixedUpdate()
    {
        timeWaited++;
        timeWaited %= waitTime;

        if(timeWaited == 0)
        {
            int value = (int) (Random.value * eligibleLocations.Count);
            Debug.Log(value + ":" + eligibleLocations.Count);
            Vector2 loc = eligibleLocations[value];
            HexTile tile = hexMap.GetHexTile((int) loc.x, (int) loc.y);
            ResourceTypeEnum rt = tile.resourceType;
            GameObject selectedBuilding = null;

            if(rt != ResourceTypeEnum.NONE)
            {
                foreach(GameObject go in buildingList.buildings)
                {
                    Building b = go.GetComponent<Building>();
                    if(b.requiresResourceOnTile != ResourceTypeEnum.NONE && b.requiresResourceOnTile == rt)
                    {
                        selectedBuilding = go;
                        break;
                    }
                }
            }

            // TODO: Make this decide based on resources etc.
            if(selectedBuilding == null)
            {
                List<GameObject> basicBuildings = new List<GameObject>();
                foreach(GameObject go in buildingList.buildings)
                {
                    Building b = go.GetComponent<Building>();
                    if(b.requiresResourceOnTile == ResourceTypeEnum.NONE) basicBuildings.Add(go);
                }

                selectedBuilding = basicBuildings[(int) (Random.value * basicBuildings.Count)];
            }

            bool success = placeBuilding(selectedBuilding,loc);

            if(success)
            {
                for(int i = 0; i <= 1; i++)
                {
                    for(int j = -1; j <= 1; j++)
                    {
                        Vector2 test = new Vector2(loc.x + i + (i == 0 & j == 0 ? -1 : 0), loc.y + j);
                        if(!eligibleLocations.Contains(test) && !visitedLocations.Contains(test))
                            eligibleLocations.Add(test);
                    }
                }
            }

            eligibleLocations.Remove(loc);
            visitedLocations.Add(loc);
        }
    }

    public bool placeBuilding(GameObject building, Vector2 location){
        Building b = building.GetComponent<Building>();

        // Check that we have enough resources
        foreach(ResourceTypeEnum rte in b.buildCost.Keys)
        {
            if(b.buildCost[rte] > resources[rte]) return false;
        }

        bool success = hexMap.GetHexTile((int) location.x, (int) location.y).PlaceBuilding(building, this);

        // Remove resources
        if(success)
        {
            foreach(ResourceTypeEnum rte in b.buildCost.Keys)
            {
                resources[rte] -= b.buildCost[rte];
            }
        }

        return success;
    }
}
