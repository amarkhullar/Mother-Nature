using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceSpawnData
{
    public ResourceTypeEnum resourceType;
    public double chanceAtPreferred = 0.2;
    public double preferredHeight = 0.5;
    public double preferredHeat = 0.5;
    public double heightDropoff = 0; // 0 for even chance
    public double heatDropoff = 0; //   0 for even chance
    public float noiseScale = 1f;

    public GameObject spawnableObject;
}
