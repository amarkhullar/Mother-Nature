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
    public int heightWaves;
    [SerializeField]
    public int heatWaves;

    private HexTile[] tiles;
    private TerrainType[] terrains;

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
        position.x = (x + z * 0.5f - z / 2) * HexTileMetrics.distBetween; // nb: z/2 and z*0.5f are different. z/2 rounds down. z*0.5f doesn't.
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
            xs[i] = tiles[i].transform.position.x;
            zs[i] = tiles[i].transform.position.z;
        }

        Wave[] heightws = NoiseGeneration.generateRandomWaves(heightWaves);
        Wave[] heatws = NoiseGeneration.generateRandomWaves(heatWaves);

        float[] heightMap = NoiseGeneration.GetNoiseAtPoints(xs, zs, heightNoiseScale, 0, 0, heightws);
        float[] heatMap = NoiseGeneration.GetNoiseAtPoints(xs, zs, heatNoiseScale, 0, 0, heatws);

        SetTerrainTypes(heightMap, heatMap);
        float[] actualHeights = SetHeights(heightMap, xs, zs);

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].transform.Translate(0, actualHeights[i] * heightMultiplier, 0);
            if(tiles[i].top != null) tiles[i].top.transform.Translate(0, actualHeights[i] * heightMultiplier, 0);
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

        Dictionary<TerrainType, Wave[]> waves = new Dictionary<TerrainType, Wave[]>();

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
