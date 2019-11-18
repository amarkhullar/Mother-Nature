using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceTypeEnum
{
    // NB: Order of these will affect order of most generated lists. Do not change without good reason. Ensure that NONE is _always_ first or things will break.

    //     Additionally, adding will shift the value of all the others down, not a problem in code unless you're doing ((ResourceTypeEnum) 3) to get WOOD,
    //     but this will screw up values in the unity editor, as those must be saved by index.
    //     Notable places to check: Generation prefabs (ResourceObject.resourceType), Building prefabs (Building.buildCostInita,resourceConsumptionInita)
    NONE,
    GOLD,
    FOOD,
    WOOD,
    STONE,
    METAL,
    ELECTRICITY,
}
