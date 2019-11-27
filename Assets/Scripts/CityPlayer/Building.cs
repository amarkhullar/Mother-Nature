using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

    // Some temporary attributes, TODO: check what's actually needed etc.
    [SerializeField] public bool destroysResourceOnPlace; // this can probably be replaced by a "yes, unless resource appears in consumption"
    [SerializeField] private ResourceTypeEnum[] resourceConsumptionInita = null; // workaround bc unity hates dicts >:[
    [SerializeField] private float[] resourceConsumptionInitb = null;
    [SerializeField] private ResourceTypeEnum[] buildCostInita = null;
    [SerializeField] private float[] buildCostInitb = null;
    [SerializeField] public ResourceTypeEnum requiresResourceOnTile = ResourceTypeEnum.NONE ;
    // pollution/sec? resource usage/sec? (ie on the tile, so like basic logger may use 1wood/sec and produce 1/sec, but super eco logger xtreme might be 0.5/sec for 5/sec)

    [SerializeField] public int maxHealth;
    public float curHealth;
    public float productivityRate = 1f;

    public Dictionary<ResourceTypeEnum, float> resourceConsumption = new Dictionary<ResourceTypeEnum,float>(); // -ve value for production
    public Dictionary<ResourceTypeEnum, float> buildCost = new Dictionary<ResourceTypeEnum, float>(); // -ve value for production

    public CityPlayer owner;

    public void Start(){
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
      //      Destroy(this.gameObject);
        }

        // Change player resources
        foreach (ResourceTypeEnum rte in resourceConsumption.Keys)
        {
            owner.resources[rte] -= resourceConsumption[rte] * Time.deltaTime * productivityRate;
        }
    }

    public void OnDeconstruction()
    {
        // Regain some resources from the building cost? Maybe 25%? Really discourage dismantling?
    }
}
