using System;
using UnityEngine;

[System.Serializable]
public class TerrainType
{
    public string name;
    public float heightCutoff; // Ideal would be removing this, replacing it with some frequency?
                               // This would then automatically be set
    public GameObject spawnableObject;
    public AnimationCurve heightMapping;
    public Color color;
}
