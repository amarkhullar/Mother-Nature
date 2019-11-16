using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    // Some temporary attributes, TODO: check what's actually needed etc.
    [SerializeField] public bool destroysResourceOnPlace;
    [SerializeField] public Dictionary<ResourceTypeEnum, int> resourceConsumption; // -ve value for production
    [SerializeField] public Dictionary<ResourceTypeEnum, int> buildCost; // -ve value for production
    public CityPlayer owner;

    public void Update()
    {
        // Change player resources by
    }

    public void OnDestroy()
    {

    }
}
