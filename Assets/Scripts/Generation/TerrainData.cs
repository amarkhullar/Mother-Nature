using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TerrainData : Singleton<TerrainData>
{
    // TODO: Probably turn this into enums etc, only once it's more set in stone though.
    [SerializeField]
    private TerrainType[] terrainTypes;

    public TerrainType[] GetTerrainTypes()
    {
        return terrainTypes;
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
