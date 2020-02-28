using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rudimentary AI, essentially just plays
public class AICityPlayer : CityPlayer
{
    private List<Vector2> eligibleLocations = new List<Vector2>();
    private List<Vector2> visitedLocations = new List<Vector2>();
    private Dictionary<Vector2, Building> ownedBuildings = new Dictionary<Vector2, Building>();
    private Vector2 startingLocation;
    private HexTileMapGenerator hexMap;
    private float cityMaxRadius = 0;

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
        startingLocation = startLocation;
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
            /// SELECT TILE TO BUILD ON
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

            /// ATTEMPT TO PLACE A BUILDING
            bool success = PlaceBuilding(selectedBuilding,loc);

            /// IF WE SUCCEEDED
            if(success)
            {
                /// ADD NEW TILES TO THE LIST OF ELIGIBLE
                List<HexTile> hs = hexMap.GetNeighbours(loc);
                foreach(HexTile h in hs)
                {
                    Vector2 test = h.GetLocation();
                    if (!eligibleLocations.Contains(test) && !visitedLocations.Contains(test))
                    {
                        eligibleLocations.Add(test);
                        h.SetMaterialColor(Color.cyan); /// DEBUG
                    }
                }
                tile.SetMaterialColor(Color.black); /// DEBUG

                /// UPDATE REST OF CITY IF NEEDED (eg: mark buildings to become skyscrapers)
                if(Vector2.Distance(startingLocation, loc) > cityMaxRadius)
                {
                    cityMaxRadius = Vector2.Distance(startingLocation, loc);
                    // Add relevant buildings to upgrade list?
                    // Increase size of skyscrapers?
                }
            }
            else
            {
                tile.SetMaterialColor(Color.red); /// DEBUG
            }

            eligibleLocations.Remove(loc);
            visitedLocations.Add(loc);
        }
    }

    // Returns a pair of (location, building to put there). If upgrade is selected then atm it'll place a skyscraper
    public (Vector2, GameObject) GetNextLocation()
    {


        return (new Vector2(), null);
    }

    public bool PlaceBuilding(GameObject building, Vector2 location){
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
