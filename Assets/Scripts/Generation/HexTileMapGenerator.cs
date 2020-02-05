using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileMapGenerator : MonoBehaviour
{

    // TODO: See if values need tweaking
    //       Generate rocks/metal veins/any other
    //       Possible ways to generate: standard uniform random above n
    //                                  in clusters, maybe with more perlin noise?
    //                                  do metals spawn in place of stone, or are they completely separate?
    //       Needs to be discussed ^^^
    //       Allow for a static seed to be used.

    [SerializeField]
    public TerrainData data;
    [SerializeField]
    public HexTile hexTile;
    [SerializeField]
    public int mapWidth;
    [SerializeField]
    public int mapHeight;
    [SerializeField]
    public float heightMultiplier;
    [SerializeField]
    public float heightNoiseScale;
    [SerializeField]
    public float heatNoiseScale;
    [SerializeField]
    public int heightWaves = 1;
    [SerializeField]
    public int heatWaves = 1;
    [SerializeField]
    public int resourceWaves = 1;
    [SerializeField]
    public int generationSeed;
    public int seedRotation = 1;


    private HexTile[] tiles;
    private TerrainType[] terrains;
    private Dictionary<ResourceTypeEnum, ResourceSpawnData> resourceData;

    // Start is called before the first frame update
    void Awake()
    {
        CreateTiles();
        LoadTerrainData();
        GenerateMap();
    }

    void LoadTerrainData()
    {
        terrains = data.GetTerrainTypes();
        resourceData = data.GetResourceData();
    }

    void CreateTiles()
    {
        tiles = new HexTile[mapHeight * mapWidth];
        for (int z = 0, i = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                CreateTile(x, z, i++);
            }
        }
    }

    void CreateTile(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * HexTileMetrics.distBetween; // nb: z/2 and z*0.5f are different. z/2 rounds down (because ints). z*0.5f doesn't.
        position.y = 0f;                                                  // ^^ this means that you end up with a slightly staggered pattern.
        position.z = z * (HexTileMetrics.distBetween - 1f);

        HexTile tile = tiles[i] = Instantiate<HexTile>(hexTile);
        tile.transform.SetParent(transform, false);
        tile.transform.localPosition = position;
        tile.x = x;
        tile.z = z;
    }

    void GenerateMap()
    {
        float[] xs = new float[mapWidth * mapHeight];
        float[] zs = new float[mapWidth * mapHeight];

        for(int i = 0; i < tiles.Length; i++)
        {
            xs[i] = tiles[i].x;
            zs[i] = tiles[i].z;
        }

        Wave[] heightws = NoiseGeneration.generateWaves(heightWaves, generationSeed);
        generationSeed += seedRotation;
        Wave[] heatws = NoiseGeneration.generateWaves(heatWaves, generationSeed);
        generationSeed += seedRotation;

        float[] heightMap = NoiseGeneration.GetNoiseAtPoints(xs, zs, heightNoiseScale, 0, 0, heightws);
        float[] heatMap = NoiseGeneration.GetNoiseAtPoints(xs, zs, heatNoiseScale, 0, 0, heatws);

        SetTerrainTypes(heightMap, heatMap);
        float[] actualHeights = SetHeights(heightMap, xs, zs);

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].heat = heatMap[i];
            tiles[i].height = heightMap[i];
            tiles[i].transform.Translate(0, actualHeights[i] * heightMultiplier, 0);
            if(tiles[i].top != null) tiles[i].top.transform.Translate(0, actualHeights[i] * heightMultiplier, 0);
        }

        GenerateResources(heightMap, heatMap);
    }

    void GenerateResources(float[] heightMap, float[] heatMap)
    {
        float[] xs = new float[mapWidth * mapHeight];
        float[] zs = new float[mapWidth * mapHeight];

        for(int i = 0; i < tiles.Length; i++)
        {
            xs[i] = tiles[i].x;
            zs[i] = tiles[i].z;
        }

        Dictionary<ResourceTypeEnum, float[]> noise = new Dictionary<ResourceTypeEnum, float[]>();

        foreach (ResourceTypeEnum rt in resourceData.Keys)
        {
            noise.Add(rt, NoiseGeneration.GetNoiseAtPoints(xs, zs, resourceData[rt].noiseScale, 0, 0, NoiseGeneration.generateWaves(resourceWaves, generationSeed)));
            generationSeed += seedRotation;
        }

        for (int i = 0; i < heightMap.Length; i++)
        {
            if(heightMap[i] < terrains[0].heightCutoff) continue; // Don't spawn anything in water.

            ResourceTypeEnum chosenType = ResourceTypeEnum.NONE;
            foreach(ResourceTypeEnum rt in noise.Keys)
            {
                double bar = resourceData[rt].chanceAtPreferred - (Math.Abs(resourceData[rt].preferredHeight - heightMap[i]) * resourceData[rt].heightDropoff)
                                                                - (Math.Abs(resourceData[rt].preferredHeat - heatMap[i]) * resourceData[rt].heatDropoff);
                //Debug.Log(bar + "::" + noise[rt][i]);
                if(noise[rt][i] < bar){
                     chosenType = rt;
                     break;
                }
            }
            if(chosenType != ResourceTypeEnum.NONE)
                tiles[i].SetResourceOnTile(resourceData[chosenType].spawnableObject);
        }
    }

    void SetTerrainTypes(float[] heightMap, float[] heatMap)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].SetTerrainType(data.GetTerrainAt(heightMap[i], heatMap[i]));
        }
    }

    float[] SetHeights(float[] heightMap, float[] xs, float[] zs)
    {
        float[] actualHeights = new float[heightMap.Length];

        // Dictionary<TerrainType, Wave[]> waves = new Dictionary<TerrainType, Wave[]>();

        for (int i = 0; i < heightMap.Length; i++)
        {
            TerrainType ter = tiles[i].terrain;

            Vector3 pos = tiles[i].transform.position;
            actualHeights[i] = (ter.flat ? ter.heightCutoff : heightMap[i]) * ter.heightScale - ter.heightScale/(2f+ter.heightCutoff/2f) + ter.heightShift;
            // The above attempts to normalise it back nearer 0. HeightShift does fine tuning to match layers with different
        }

        return actualHeights;
    }

    void Start()
    {
        GenerateMeshes();
    }

    void GenerateMeshes()
    {
        foreach(HexTile hx in tiles)
        {
            if (hx.isAlive) hx.GenerateMesh();
        }
    }

    public HexTile GetHexTile(int x, int z)
    {
        return this.tiles[x + z * mapWidth];
    }

}
