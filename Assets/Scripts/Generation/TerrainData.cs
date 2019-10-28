using System;
using UnityEngine;

[System.Serializable]
public class TerrainData : Singleton<TerrainData>
{
    [SerializeField]
    private TerrainType[] terrainsTypes;

    public TerrainType[] GetTerrainTypes()
    {
        return terrainsTypes;
    }

}