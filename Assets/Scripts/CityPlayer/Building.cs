using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

    // Some temporary attributes, TODO: check what's actually needed etc.
    [SerializeField] public bool destroysResourceOnPlace;
    [SerializeField] private ResourceTypeEnum[] resourceConsumptionInita; // workaround bc unity hates dicts >:[
    [SerializeField] private float[] resourceConsumptionInitb;
    [SerializeField] private ResourceTypeEnum[] buildCostInita;
    [SerializeField] private float[] buildCostInitb;

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
    }

    public void Update()
    {
        // Change player resources
        foreach(ResourceTypeEnum rte in resourceConsumption.Keys)
        {
            owner.resources[rte] -= resourceConsumption[rte] * Time.deltaTime;
        }
    }

    public void OnDestroy()
    {

    }
}
