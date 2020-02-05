using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TerrainData : Singleton<TerrainData>
{
    [SerializeField]
    public TerrainType[] terrainTypes;
    [SerializeField]
    private ResourceSpawnData[] resourceSpawnData = null; // So that vv can be editted from the unity editor. It doesn't display Dicts.

    public Dictionary<ResourceTypeEnum, ResourceSpawnData> resourceData = new Dictionary<ResourceTypeEnum, ResourceSpawnData>();


    public TerrainType[] GetTerrainTypes()
    {
        return terrainTypes;
    }

    public Dictionary<ResourceTypeEnum, ResourceSpawnData> GetResourceData()
    {
        for(int i = 0; i < resourceSpawnData.Length; i++)
        {
            resourceData.Add(resourceSpawnData[i].resourceType, resourceSpawnData[i]);
        }
        return resourceData;
    }

    public (TerrainType,int) GetTerrainAt(float height, float heat)
    {
        TerrainType n = terrainTypes[0];
        for (int i = 1; i < terrainTypes.Length; i++)
        {
            if ((terrainTypes[i].heightCutoff > height && terrainTypes[i].heightCutoff < n.heightCutoff) ||
                (terrainTypes[i].heightCutoff > n.heightCutoff && n.heightCutoff < height))
                    n = terrainTypes[i];
        }

        int var = 0;
        if(n.heatCutoffs.Length > 0)
        {
            for(int i = 1; i < n.heatCutoffs.Length; i++)
            {
                if ((n.heatCutoffs[i] > n.heatCutoffs[var] && n.heatCutoffs[i] < heat) ||
                    (n.heatCutoffs[i] > n.heatCutoffs[var] && n.heatCutoffs[var] < heat))
                        var = i;
            }
        }

        return (n,var);
    }

}
