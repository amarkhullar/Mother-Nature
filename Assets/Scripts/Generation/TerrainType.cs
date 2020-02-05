using System;
using UnityEngine;

[System.Serializable]
public class TerrainType
{
    // TODO: Maybe change this to a hight range, heat range, and separate out the []'s
    public string name;
    public float heightCutoff;
    public float heightScale;
    public float heightShift;
    public bool flat;

    public float[] heatCutoffs; // Heat changes variation of the terraintype. Eg: Desert -> Plain -> Forest -> Snowy forest, all have same height
    public Color[] color;
    
}
