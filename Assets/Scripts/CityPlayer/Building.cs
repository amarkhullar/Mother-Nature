using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

    // Some temporary attributes, TODO: check what's actually needed etc.
    [SerializeField] public bool destroysResourceOnPlace; // this can probably be replaced by a "yes, unless resource appears in consumption"
    [SerializeField] public ResourceTypeEnum[] resourceConsumptionInita = null; // workaround bc unity hates dicts >:[
    [SerializeField] public double[] resourceConsumptionInitb = null;
    [SerializeField] public ResourceTypeEnum[] buildCostInita = null;
    [SerializeField] public double[] buildCostInitb = null;
    [SerializeField] public ResourceTypeEnum requiresResourceOnTile = ResourceTypeEnum.NONE;
    [SerializeField] public double pollutionPerSecond = 0;
    [HideInInspector] public Vector2 location;
    [HideInInspector] public HexTile tile;
    // pollution/sec? resource usage/sec? (ie on the tile, so like basic logger may use 1wood/sec and produce 1/sec, but super eco logger xtreme might be 0.5/sec for 5/sec)

    [SerializeField] public int maxHealth = 100;
    public double curHealth;
    public double productivityRate = 1;
    public double tileResourceUsage = 0;

    // If we run out of resources. TODO: Double check this system. It should work okay, but may flip flop between full productivity for 1 resource then off.
    private double prevProductivityRate = 1;
    private bool stopped = false;
    public bool placedOnWater = false;

    public Dictionary<ResourceTypeEnum, double> resourceConsumption = new Dictionary<ResourceTypeEnum,double>(); // -ve value for production
    public Dictionary<ResourceTypeEnum, double> buildCost = new Dictionary<ResourceTypeEnum, double>(); // -ve value for production

    [HideInInspector] public CityPlayer owner;

    public void Awake(){
        // Transfers the init arrays to the dicts. Yuck.
        if(resourceConsumptionInitb.Length != resourceConsumptionInita.Length) Debug.LogWarning("you've done goof v1: mismatched array lengths");
        for(int i = 0; i < resourceConsumptionInita.Length; i++)
        {
            resourceConsumption.Add(resourceConsumptionInita[i], resourceConsumptionInitb[i]);
        }

        if(buildCostInitb.Length != buildCostInita.Length) Debug.LogWarning("you've done goof v2: mismatched array lengths");
        for(int i = 0; i < buildCostInita.Length; i++)
        {
            buildCost.Add(buildCostInita[i], buildCostInitb[i]);
        }

        curHealth = maxHealth;
    }

    public void Update()
    {
        if(owner == null)
        {
            Debug.LogWarning("Owner was never set, destroying building.");
            // Panic
            Destroy(this.gameObject);
        }

        if(curHealth <= 0)
        {
            Destroy(this.gameObject);
        }

        // Change player resources
        bool enough = true;
        foreach (ResourceTypeEnum rte in resourceConsumption.Keys)
        {
            owner.resources[rte] -= resourceConsumption[rte] * Time.deltaTime * productivityRate;
            if(owner.resources[rte] < 0)
            {
                owner.resources[rte] = 0;
                enough = false;
            }
        }

        if(tileResourceUsage != 0)
        {
            if(tile.resourceAmount > 0)
                tile.resourceAmount -= tileResourceUsage * Time.deltaTime * productivityRate;

            else
                enough = false; // IE: Building is spent up.
        }

        // Ran out of resources
        if(!enough && !stopped)
        {
            prevProductivityRate = productivityRate;
            productivityRate = 0;
            stopped = true;
        }

        // Regained Resources
        if(enough && stopped)
        {
            productivityRate = prevProductivityRate;
            stopped = false;
        }

    }

    public void OnDeconstruction()
    {
        // Regain some resources from the building cost? Maybe 25%? Really discourage dismantling?
        double regainRate = 0.25;
        foreach (ResourceTypeEnum rte in buildCost.Keys)
        {
            owner.resources[rte] += buildCost[rte] * regainRate;
        }
    }
}
