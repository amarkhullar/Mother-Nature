using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// TODO: If this becomes complex enough create a class/enum Action that says whether to build/destroy/upgrade etc
// TODO: build over roads (should work?), build over buildings close to centre, increase height of centre buildings
// TODO: Start with some houses maybe?
// TODO: Give them a flat +0.1 for food/wood at all times maybe? To stop bad spawns rendering it incapable of doing anything. (done)
// TODO: Selective deliberate production decreases for buildings which use high priority resources?
public class AICityPlayer : CityPlayer
{
    private List<Vector2> eligibleLocations = new List<Vector2>();
    private List<Vector2> visitedLocations = new List<Vector2>();
    private Dictionary<Vector2, Building> ownedBuildings = new Dictionary<Vector2, Building>();
    private HexTileMapGenerator hexMap;
    private float cityMaxRadius = 0;
    private float maxViewDistance = 10;

    public float pollutionAversion = 1f; // 0 = doesn't give af, -ve = actively wants to pollute, +ve = tries not to
    public float cityViewMultiplier = 6f;
    public Vector2 startingLocation;

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
        visitedLocations.Add(startingLocation);
        timeWaited = waitTime - 50;

        // Place city center, which should always be buildinglist.buildings[1]
        Debug.Log(PlaceBuilding(buildingList.buildings[1], startingLocation));
        foreach (HexTile h in hexMap.GetNeighbours(startingLocation))
        {
            if (visitedLocations.Contains(h.GetLocation())) continue;
            eligibleLocations.Add(h.GetLocation());
            h.SetMaterialColor(Color.cyan); /// DEBUG
        }

        // Setup starting resources
        //         none food wood stone coal metal electricity water
        int[] start = { 0, 50, 50,  10,  0,   0,   0,     0 };
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
                return;
            }
            Vector2 loc = tile.GetLocation();

            /// ATTEMPT TO PLACE A BUILDING
            bool success = PlaceBuilding(selectedBuilding,loc);

            /// IF WE SUCCEEDED
            if(success)
            {
                Building bld = selectedBuilding.GetComponent<Building>();
                foreach(ResourceTypeEnum rte in bld.buildCostInita)
                {
                    resources[rte] -= bld.buildCostInitb[Array.IndexOf(bld.buildCostInita, rte)];
                }

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
                    maxViewDistance = 10;//cityMaxRadius * cityViewMultiplier; * desperation?
                    // Add relevant buildings to upgrade list?
                    // Increase size of skyscrapers?
                }

                if(bld != buildingList.buildings[0]) // If it's not a road.
                {
                    eligibleLocations.Remove(loc);
                    visitedLocations.Add(loc);
                }
            }
            else{
                Debug.Log("Placing a building failed. Removing tile future considerations.");
                tile.SetMaterialColor(Color.red); /// DEBUG

                visitedLocations.Add(loc); // Probably done something stupid like trying to place a building on water. This invalidates the tile in the future.
            }
        }
    }

    // Returns a pair of (location, building to put there). If upgrade is selected then atm it'll place a skyscraper
    public (HexTile, GameObject) GetNextAction()
    {
        // Check resources and see which we need to prioritise.
        Dictionary<ResourceTypeEnum, double> generating = GetResourceGeneration();
        Dictionary<ResourceTypeEnum, double> priorities = GetResourcePriorities(generating);
        ResourceTypeEnum prioResourceType = ResourceTypeEnum.NONE;
        double prioCutoffPoint = 2; // What does the lowest priority have to be before we focus on it?

        var orderedpriors = priorities.OrderBy(i => i.Value);
        prioResourceType = orderedpriors.ElementAtOrDefault(0).Key;

        //// DEBUG:
        String d = "Resource Prios: ", p = "", r = "";
        foreach(ResourceTypeEnum rte in priorities.Keys)
        {
            p = priorities[rte] + "";
            r = resources[rte] + "";
            d += rte.ToString() + ": " + p.Substring(0,4 > p.Length ? p.Length : 4) + ", " + r.Substring(0,4 > r.Length ? r.Length : 4) + ";  ";
        }
        
        Vector2 vStart = new Vector2(); // Technically I shouldn't need to set vStart as it's set in the if's, but programming sucks.
        GameObject selectedBuilding = null;

        // Now we've got the priorities, decide how to fix anything that's running low.
        // Two paths:
        //  A priority:
        //      1, find the nearest resource node
        //      2, find best building depending on that
        //      3, update variables to reflect whether we're pathing to resource or not.
        //  No real priority: Make a random Building


        ///// IF WE HAVE A PRIORITY /////
        if(prioResourceType != ResourceTypeEnum.NONE && priorities[prioResourceType] < prioCutoffPoint) {
            // 1:
            (HexTile closestRT, double closestDist) = FindNearestResourceNode(prioResourceType);

            // 2:
            selectedBuilding = GetHighestRatedBuilding(prioResourceType, priorities, closestDist); // shouldn't need a check on closestDist, if closestRT is null, closestDist should still be ridiculously large

            // Sanity Check
            if(selectedBuilding == null || (selectedBuilding.GetComponent<Building>().requiresResourceOnTile != ResourceTypeEnum.NONE && closestRT == null))
            {
                Debug.Log("WARNING: No best rated building?! Aborting. ::" + prioResourceType + ":" + priorities[prioResourceType]);
                return (null, null);
            }

            // 3:
            bool ontile = selectedBuilding.GetComponent<Building>().requiresResourceOnTile != ResourceTypeEnum.NONE;
            vStart = ontile ? closestRT.GetLocation() : startingLocation;
            if(ontile && !this.eligibleLocations.Contains(closestRT.GetLocation()))
                selectedBuilding = buildingList.buildings[0]; // If not literally right next to it, place a Road.
            
        }
        ///// \IF WE HAVE A PRIORITY\ /////


        ///// NO PRIORITY ///// (or if the above failed)
        if (prioResourceType == ResourceTypeEnum.NONE || priorities[prioResourceType] > prioCutoffPoint)
        {
            // Select random building
            // TODO: Make it less random, maybe have upgrade buildings here?
            List<GameObject> eligibleBuildings = new List<GameObject>();
            foreach (GameObject go in buildingList.buildings)
            {
                Building b = go.GetComponent<Building>();
                if (b.requiresResourceOnTile == ResourceTypeEnum.NONE && affordable(b) && !b.name.Equals("road"))
                {
                    eligibleBuildings.Add(go);
                }
            }
            if(eligibleBuildings.Count != 0) selectedBuilding = eligibleBuildings[(int) (UnityEngine.Random.value * eligibleBuildings.Count-1)];// -1)+1 to stop it being a Road
            else{
                Debug.Log("No eligable buildings? Aborting.");
                return (null, null);
            }

            vStart = startingLocation;
        }
        //// \NO PRIORITY\ /////

        HexTile selectedTile = GetClosestEligableTile(vStart, selectedBuilding.GetComponent<Building>());
       
        Debug.Log(d + ":: " + selectedBuilding.name);

        return (selectedTile, selectedBuilding);
    }

    public bool PlaceBuilding(GameObject building, Vector2 location){

        GameObject success = hexMap.GetHexTile((int) location.x, (int) location.y).PlaceBuilding(building, this);

        // Remove resources
        if(success != null)
        {
            Building b = success.GetComponent<Building>();
            foreach(ResourceTypeEnum rte in b.buildCost.Keys)
            {
                resources[rte] -= b.buildCost[rte];
            }

            ownedBuildings.Add(location, b);
        }

        return success != null;
    }

    private Dictionary<ResourceTypeEnum, double> GetResourceGeneration()
    {
        // Loop over every building and add the amount they're generating.
        Dictionary<ResourceTypeEnum, double> generating = new Dictionary<ResourceTypeEnum, double>();
        foreach (ResourceTypeEnum rte in Enum.GetValues(typeof(ResourceTypeEnum)))
            generating[rte] = 0;

        foreach (Building b in ownedBuildings.Values)
        {
            foreach (ResourceTypeEnum rte in b.resourceConsumption.Keys)
            {
                generating[rte] += b.productivityRate * -b.resourceConsumption[rte];
            }
        }

        return generating;
    }

    private (HexTile, double) FindNearestResourceNode(ResourceTypeEnum prioResourceType)
    {
        double dist = 10000;
        HexTile closestRT = null;
        foreach (Vector2 elv in eligibleLocations)
        {
            if (hexMap.GetHexTile(elv).resourceType == prioResourceType)
            {
                if ((elv - startingLocation).magnitude < dist)
                {
                    dist = (elv - startingLocation).magnitude;
                    closestRT = hexMap.GetHexTile(elv);
                }
            }
        }

        if (closestRT == null)
        {
            List<HexTile> inrange = hexMap.GetTilesWithinRange((int)startingLocation.x, (int)startingLocation.y, (int)maxViewDistance);
            inrange.RemoveAll(item => visitedLocations.Contains(item.GetLocation()));
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
        }

        return (closestRT, dist);
    }

    private Dictionary<ResourceTypeEnum, double> GetResourcePriorities(Dictionary<ResourceTypeEnum, double> generating)
    {
        // Algorithm to decide the priorities of the different resources, keeping in mind:
        //  a) current amount
        //  b) rate of generation
        //  c) biases of what we think is most important

        // TODO: Probably rewrite this and change the biases

        //               none food wood stone coal metal electricity water
        double[] bias = { 100, -2,  -3,   0,    1,   1,     1.5,      100 };
        Dictionary<ResourceTypeEnum, double> priorities = new Dictionary<ResourceTypeEnum, double>();

        int biasindex = 0;
        foreach (ResourceTypeEnum rte in Enum.GetValues(typeof(ResourceTypeEnum)))
        { // Lower = higher priority. Mix of how fast we're generating/losing vs how much we have vs how important it is
            priorities[rte] = (generating.ContainsKey(rte) ? generating[rte] : 0) * 0.7f + (resources[rte] - 20) / 30 + bias[biasindex++];
        }

        return priorities;
    }

    private GameObject GetHighestRatedBuilding(ResourceTypeEnum prioResourceType, Dictionary<ResourceTypeEnum, double> priorities, double dist)
    {
        // Alg to rate all the buildings based on:
        //  a) Which resource we consider most important
        //  b) which other resources may also be important soon
        //  c) whether we can use a nearby resource node
        //  d) TODO: pollution values compared to willingness to pollute
        // NB: Having to use the inits as the Dicts aren't yet initialised. >:c
        // NB2: Higher rating = better
        // Vars:
        double buildScale = 0.5, distScale = 0.3, pollScale = 0.5;

        GameObject selectedBuilding = null;
        double rating, highest = 0;
        Building bld;
        foreach (GameObject go in buildingList.buildings)
        {
            rating = 0;
            bld = go.GetComponent<Building>();

            // If the building doesn't generate the resource, ignore it and continue on to the next building.
            if ((!bld.resourceConsumptionInita.Contains(prioResourceType)) || bld.resourceConsumptionInitb[Array.IndexOf(bld.resourceConsumptionInita, prioResourceType)] > 0 || !affordable(bld)) continue;
            if (!affordable(bld)) continue;

            // For all resources w prio < 0 (with bias to higher prios) check whether this generates or consumes.
            foreach (ResourceTypeEnum rte in bld.resourceConsumptionInita)
            {
                if (priorities[rte] < 0)
                {   // -ve * -ve * -ve, if generating, so "subtracted" from rating.
                    rating -= priorities[rte] * priorities[rte] * bld.resourceConsumptionInitb[Array.IndexOf(bld.resourceConsumptionInita, rte)];
                }
            }

            // For all resources w prio < 0, check how much this consumes
            foreach (ResourceTypeEnum rte in bld.buildCost.Keys)
            {
                if (priorities[rte] < 0)
                {
                    rating += priorities[rte] * bld.buildCost[rte] * buildScale;
                }
            }

            if (bld.requiresResourceOnTile != ResourceTypeEnum.NONE) // If we have to go 20 tiles to place down this building, is it really better than the alternative(s)?
            {
                rating -= dist > 0 ? dist * distScale : 1000; // TODO: Test with different constants. The 1000 essentially means that it's guarenteed to fail if requires resource that cannot be found
            }

            // Factor in pollution vs how happy we are to pollute.
            rating -= bld.pollutionPerSecond * pollutionAversion * pollScale;

            if (highest < rating)
            {
                highest = rating;
                selectedBuilding = go;
            }
        }
        return selectedBuilding;
    }

    private HexTile GetClosestEligableTile(Vector2 vStart, Building bldng)
    {
        Vector2 vToFind = new Vector2();
        double dist = 10000;

        // Closest tile to center/closestResourceTile
        foreach (Vector2 v in eligibleLocations)
        {
            if (dist > (vStart - v).magnitude && hexMap.GetHexTile(v).resourceType == bldng.requiresResourceOnTile)
            {
                dist = (vStart - v).magnitude;
                vToFind = v;
            }
        }

        return hexMap.GetHexTile(vToFind);
    }

    private bool affordable(Building building)
    {
        foreach(ResourceTypeEnum rt in building.buildCostInita)
            if (resources[rt] < building.buildCostInitb[Array.IndexOf(building.buildCostInita, rt)])
                return false;

        return true;
    }

}
