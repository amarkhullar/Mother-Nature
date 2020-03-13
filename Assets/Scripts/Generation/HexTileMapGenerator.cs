using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileMapGenerator : MonoBehaviour
{

    // TODO: See if values need tweaking
    // TODO: Reshape map, and in doing so fix all the indexing

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
        int zstart = 0;
        for (int x = 0, i = 0; x < mapWidth; x++)
        {
            for (int z = zstart; z < mapHeight - zstart; z++)
            {
                CreateTile(x, z, i++);
            }
            //zstart -= 1;
        }
    }

    void CreateTile(int x, int z, int i)
    {
        Vector3 position;
        // position.x = (x + z * 0.5f - z / 2) * HexTileMetrics.distBetween; // nb: z/2 and z*0.5f are different. z/2 rounds down (because ints). z*0.5f doesn't.
        position.x = (x + z * 0.5f) * HexTileMetrics.distBetween;
        position.y = 0f;                                                  // ^^ this means that you end up with a slightly staggered pattern.
        position.z = z * (HexTileMetrics.distBetween - 1f);

        HexTile tile = tiles[i] = Instantiate<HexTile>(hexTile);
        tile.transform.SetParent(transform, false);
        tile.transform.localPosition = position;
        tile.x = x;
        tile.z = z;
    }

    // NB: If how the tiles are ordered changes, the map will change to follow that
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

    public HexTile GetHexTile(Vector2 v)
    {
        return GetHexTile((int) v.x, (int) v.y);
    }

    public HexTile GetHexTile(int x, int z)
    {
        int i = z + x * mapWidth;
        if (i < tiles.Length && i > 0)
            return this.tiles[z + x * mapWidth];
        return null;
    }

    public List<HexTile> GetNeighbours(Vector2 v)
    {
        return GetNeighbours((int) v.x, (int) v.y);
    }

    public List<HexTile> GetNeighbours(int x, int z)
    {
        List<HexTile> ns = new List<HexTile>();

        // this looks a bit odd but it's the simplest way I found to do it.
        // Coordinates: (it loops over top to bottom)
        //                  /^/ z
        //      (-1,1)  (0,1)                    Start top left, loop over l->r, t->b
        //   (-1,0) (0,0)  (1,0)   --> x         When it hits (0,0) it shifts the x starting coord over by 1
        //      (0,-1)  (1,-1)                   You can think of it as going down a 3x2 and being shoved over a column half way through

        int istart = -1;
        for(int j = 1; j >= -1; j--)
        {
            for(int i = istart; i <= istart+1; i++)
            {
                if (i == 0 && j == 0)
                {
                    istart += 1;
                    i += 1;
                }

                if(x + i >= 0 && z + j >= 0 && x + i < mapWidth && z + j < mapHeight)
                  ns.Add(GetHexTile(x + i, z + j));
            }
        }

        return ns;
    }

    public List<HexTile> GetTilesWithinRange(int x, int z, int dist)
    {
        // This is a much better way than above.

        List<HexTile> ns = new List<HexTile>();
        HexTile tmp;
        for(int i = -dist; i <= dist; i++)
        {
            for(int j = (int) Math.Max(-dist, -i-dist); j <= (int) Math.Min(dist, -i+dist); j++)
            {
                if(i != 0 || j != 0)
                {
                    tmp = GetHexTile(x + i, z + j);
                    if(tmp != null)
                       ns.Add(tmp);
                }
            }
        }
        return ns;
    }

}
