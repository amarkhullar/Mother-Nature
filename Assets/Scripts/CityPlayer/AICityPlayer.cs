using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// TODO: If this becomes complex enough create a class/enum Action that says whether to build/destroy/upgrade etc
public class AICityPlayer : CityPlayer
{
    private List<Vector2> eligibleLocations = new List<Vector2>();
    private List<Vector2> visitedLocations = new List<Vector2>();
    private Dictionary<Vector2, Building> ownedBuildings = new Dictionary<Vector2, Building>();
    public Vector2 startingLocation;
    public float cityViewMultiplier = 6f;
    private HexTileMapGenerator hexMap;
    private float cityMaxRadius = 0;
    private float maxViewDistance = 10;

    // TODO: Change wait time to be a function of city size.
    public int waitTime = 250; // FixedUpdates between actions, 50=1s by default
    private int timeWaited; // First move is a second after the start

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        hexMap = GameObject.Find("CPTileGen").GetComponent<HexTileMapGenerator>();
        if (startingLocation == null || startingLocation.x == -1f || startingLocation.y == -1f)
        {
            Vector2 startLocation = new Vector2();
            startLocation.x = UnityEngine.Random.value * hexMap.mapWidth;
            startLocation.y = UnityEngine.Random.value * hexMap.mapHeight;
            HexTile t = hexMap.GetHexTile((int)startLocation.x, (int)startLocation.y);
            while (t.terrain.name.Equals("Water"))
            {
                startLocation.x = UnityEngine.Random.value * hexMap.mapWidth;
                startLocation.y = UnityEngine.Random.value * hexMap.mapHeight;
                t = hexMap.GetHexTile((int)startLocation.x, (int)startLocation.y);
            }

            Debug.Log(startLocation + ":" + hexMap.mapWidth + ":" + hexMap.mapHeight);
            startingLocation = startLocation;
        }
        eligibleLocations.Add(startingLocation);
        timeWaited = waitTime - 50;

        //         none food wood stone coal metal electricity water
        int[] start = { 0, -3, -5,   0,   1,   1,    2,     0 };
        int starti = 0;
        foreach (ResourceTypeEnum rte in Enum.GetValues(typeof(ResourceTypeEnum))){
            resources[rte] = start[starti++];
        }
    }

    new void Update() { }// Override to stop the mouse stuff.

    // Fixed Update is called 50 times/sec by default, which is the rate of physics calculations.
    void FixedUpdate()
    {
        timeWaited++;
        timeWaited %= waitTime;

        if(timeWaited == 0)
        {

            (HexTile tile, GameObject selectedBuilding) = GetNextAction();
            if(tile == null || selectedBuilding == null)
            {
                Debug.Log("no tile/building pair found");
            }
            Vector2 loc = tile.GetLocation();

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
                    maxViewDistance = 10;//cityMaxRadius * cityViewMultiplier;
                    // Add relevant buildings to upgrade list?
                    // Increase size of skyscrapers?
                }
            }

            eligibleLocations.Remove(loc);
            visitedLocations.Add(loc);
        }
    }

    // Returns a pair of (location, building to put there). If upgrade is selected then atm it'll place a skyscraper
    public (HexTile, GameObject) GetNextAction()
    { // TODO: find a way to clean this up or separate it into different functions.
        // Check resources
        Dictionary<ResourceTypeEnum, double> priorities = new Dictionary<ResourceTypeEnum, double>();
        Dictionary<ResourceTypeEnum, double> generating = new Dictionary<ResourceTypeEnum, double>();
        //             none food wood stone coal metal electricity water
        double[] bias = {0, -3, -3, 0, 1, 1, 2, 0};


        foreach(Building b in ownedBuildings.Values)
        {
            foreach(ResourceTypeEnum rte in b.resourceConsumption.Keys){
                generating[rte] += b.productivityRate * b.resourceConsumption[rte];
            }
        }

        int biasindex = 0;
        ResourceTypeEnum prioResourceType = ResourceTypeEnum.NONE;
        foreach (ResourceTypeEnum rte in Enum.GetValues(typeof(ResourceTypeEnum)))
        { // Lower = higher priority. Mix of how fast we're losing vs how much we have vs how important it is
            priorities[rte] = (generating.ContainsKey(rte) ? generating[rte] : 0) + resources[rte]/10 + bias[biasindex++];
        }

        var orderedpriors = priorities.OrderBy(i => i.Value);
        prioResourceType = orderedpriors.ElementAtOrDefault(0).Key;

        String s = "Resource Prios: ";
        foreach(ResourceTypeEnum rte in priorities.Keys)
        {
            s += rte.ToString() + ": " + priorities[rte] + ";  ";
        }

        Debug.Log(s);
        s = "";

        float dist = 1000;
        Vector2 vStart = new Vector2(), vToFind; // Technically I shouldn't need to set vStart as it's set in the if's, but programming sucks.
        GameObject selectedBuilding = null;

        // Now we've got the priorities, decide how to fix anything that's running low.
        // Two paths:
        //  A priority:
        //      Firstly, find the nearest resource node
        //      Secondly, find best building depending on that
        //      Thirdly, update variables to reflect whether we're pathing to resource or not.
        //  No real priority: Make a random Building


        ///// IF WE HAVE A PRIORITY /////
        int ohno = 0;
        temp_label_just_so_I_dont_have_to_refactor_any_code_just_yet:

        if(prioResourceType != ResourceTypeEnum.NONE && priorities[prioResourceType] < 0) {
            s += "1:";
            // 1:
            List<HexTile> inrange = hexMap.GetTilesWithinRange((int)startingLocation.x, (int)startingLocation.y, (int)maxViewDistance);
            inrange.RemoveAll(item => visitedLocations.Contains(item.GetLocation()));
            HexTile closestRT = null;
            foreach (HexTile ht in inrange)
            {
                if (ht.resourceType == prioResourceType)
                {
                    if ((ht.GetLocation() - startingLocation).magnitude < dist)
                    {
                        closestRT = ht;
                        dist = (ht.GetLocation() - startingLocation).magnitude;
                    }
                }
            }
            //Debug.Log((closestRT == null) + "::" + inrange.Count() + "::" + ((int) maxViewDistance));
            //Debug.Log(closestRT.GetLocation());

            // 2:
            //Dictionary<GameObject, double> buildingRating = new Dictionary<GameObject, double>();
            double rating, highest = -10000;
            Building bld;
            foreach (GameObject go in buildingList.buildings)
            {
                rating = 0;
                bld = go.GetComponent<Building>();
                if ((!bld.resourceConsumptionInita.Contains(prioResourceType)) || bld.resourceConsumptionInitb[Array.IndexOf(bld.resourceConsumptionInita, prioResourceType)] > 0 || !affordable(bld)) continue;

                // NB: Having to use the inits as the Dicts aren't yet initialised. >:c
                foreach (ResourceTypeEnum rte in bld.resourceConsumptionInita)
                {
                    if (priorities[rte] < 0)
                    {
                        rating += priorities[rte] * bld.resourceConsumptionInitb[Array.IndexOf(bld.resourceConsumptionInita, rte)]; // Both -ve so it's +ve
                    }
                }

                foreach (ResourceTypeEnum rte in bld.buildCost.Keys)
                {
                    if (priorities[rte] < 0)
                    {
                        rating += priorities[rte] * bld.buildCost[rte];
                    }
                }

                //buildingRating[go] = rating;

                if (bld.requiresResourceOnTile != ResourceTypeEnum.NONE) // If we have to go 20 tiles to place down this building, is it really better than the alternative(s)?
                {
                    rating -= (dist + (closestRT == null ? 10000 : 0)) * 0.3; // TODO: Test with different constants. As this is only run if requiresResourceonTile it doubles as a null check.
                }

                if (highest < rating)
                {
                    highest = rating;
                    selectedBuilding = go;
                }
            }

            if (selectedBuilding == null)
            {
                ohno++;
                if (ohno < 3)
                {
                    prioResourceType = orderedpriors.ElementAtOrDefault(ohno).Key;
                    goto temp_label_just_so_I_dont_have_to_refactor_any_code_just_yet;
                }
            }
            else
            {
                s += "b" + selectedBuilding.name + ":";
                bool ontile = selectedBuilding.GetComponent<Building>().requiresResourceOnTile != ResourceTypeEnum.NONE;
                vStart = ontile ? closestRT.GetLocation() : startingLocation;
                if(closestRT != null) s += "ct" + closestRT.GetLocation() + ":";

                if(ontile && !this.eligibleLocations.Contains(closestRT.GetLocation()))
                {
                    selectedBuilding = buildingList.buildings[0]; // If not literally right next to it, place a Road.
                }

                dist = 10000; // because it's reused
            }
            s += "o"+ohno + ":";
        }
        ///// \IF WE HAVE A PRIORITY\ /////


        ///// NO PRIORITY ///// (or if the above failed)
        if (prioResourceType == ResourceTypeEnum.NONE || priorities[prioResourceType] > 0 || ohno == 3)
        {
            s += "2:";
            // Select random building
            // TODO: Make it less random, maybe have upgrade buildings here?
            List<GameObject> eligibleBuildings = new List<GameObject>();
            foreach (GameObject go in buildingList.buildings)
            {
                Building b = go.GetComponent<Building>();
                if (b.requiresResourceOnTile == ResourceTypeEnum.NONE && affordable(b))
                {
                    eligibleBuildings.Add(go);
                }
            }
            selectedBuilding = eligibleBuildings[(int) (UnityEngine.Random.value * eligibleBuildings.Count-1)+1];// -1)+1 to stop it being a Road

            vStart = startingLocation;
        }
        //// \NO PRIORITY\ /////


        ///// SELECT WHERE TO BUILD /////
        HexTile selectedTile = null;
        Building bldng = selectedBuilding.GetComponent<Building>();
        vToFind = new Vector2();

        // Closest tile to center/closestResourceTile
        foreach(Vector2 v in eligibleLocations)
        {
            if(dist > (vStart - v).magnitude && hexMap.GetHexTile(v).resourceType == bldng.requiresResourceOnTile)
            {
                dist = (vStart - v).magnitude;
                vToFind = v;
            }
        }

        selectedTile = hexMap.GetHexTile(vToFind);
        ///// \SELECT WHERE TO BUILD\ /////
        Debug.Log(s);

        return (selectedTile, selectedBuilding);
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

    private bool affordable(Building building)
    {
        foreach(ResourceTypeEnum rt in building.buildCost.Keys)
            if (resources[rt] < building.buildCost[rt])
                return false;

        return true;
    }

}
