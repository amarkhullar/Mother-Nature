using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGeneration : MonoBehaviour{
    
    private TerrainType[] terrainTypes; // These are gotten from the TerrainDataHolder prefab that _must_ be a child of any object with this script.

    [SerializeField]
    public int numberOfWaves;
    private Wave[] waves; // These are randomly generated

    [SerializeField]
    public MeshRenderer tileRenderer;

    [SerializeField]
    public MeshFilter meshFilter;

    [SerializeField]
    public MeshCollider meshCollider;

    [SerializeField]
    public float mapScale;

    [SerializeField]
    public float heightMultiplier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        GetTerrainData();
        GenerateTile();
    }

    void GetTerrainData()
    {
        TerrainData td = this.GetComponentInChildren<TerrainData>();
        terrainTypes = td.GetTerrainTypes();
    }

    void GenerateTile(){
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt (meshVertices.Length);
        int tileWidth = tileDepth;

        float xoffset = -this.gameObject.transform.position.x;
        float zoffset = -this.gameObject.transform.position.z;

        waves = NoiseGeneration.generateRandomWavesSingleton(numberOfWaves);

        float[,] heightMap = NoiseGeneration.generateNoiseMap(tileDepth, tileWidth, this.mapScale, xoffset, zoffset, waves);

        // TODO: Insert some smoothing functions?

        Texture2D tileTexture = buildTexture(heightMap);
        this.tileRenderer.material.mainTexture = tileTexture;

        UpdateMeshVerticies(heightMap);
        PopulateMap(heightMap);
    }

    private Texture2D buildTexture(float[,] heightMap){
        int tileDepth = heightMap.GetLength (0);
        int tileWidth = heightMap.GetLength (1);

        Color[] colorMap = new Color[tileWidth * tileDepth];
        for(int z = 0; z < tileDepth; z++){
            for(int x = 0; x < tileWidth; x++){
                float height = heightMap[z,x];

                // Linearly interpolate between black & weight by height amount.
                colorMap[z * tileWidth + x] = GetTerrainTypeFromHeight(height).color;
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    private void UpdateMeshVerticies(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int vertIndex = 0;
        for(int z = 0; z < tileDepth; z++)
        {
            for(int x = 0; x < tileWidth; x++)
            {
                float height = fixHeightMap(heightMap[z, x]) * heightMultiplier;
                Vector3 vec = meshVertices[vertIndex];
                meshVertices[vertIndex] = new Vector3(vec.x, height, vec.z);
                vertIndex++;
            }
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshCollider.sharedMesh = this.meshFilter.mesh;

    }

    private void PopulateMap(float[,] heightMap)
    {
        // A reasonably large chunk of this is mapping height to terrain type, this has been done twice already, should deffo find a way to cache results.

        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        int vertIndex = 0;
        for (int z = 0; z < tileDepth; z++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                TerrainType type = GetTerrainTypeFromHeight(heightMap[x, z]);
                if (type.spawnableObject != null)
                {
                    if (Random.value > 0.8f)
                    {
                        Instantiate(type.spawnableObject, this.meshFilter.mesh.vertices[vertIndex] + this.transform.position, this.transform.localRotation);
                    }
                }
                vertIndex++;
            }
        }
    }

    private float fixHeightMap(float height)
    {
        TerrainType terrain = GetTerrainTypeFromHeight(height);
        return terrain.heightMapping.Evaluate(height);
    }

    // Returns terrain with the closest height about provided height, or highest if there are none.
    private TerrainType GetTerrainTypeFromHeight(float height)
    {
        TerrainType n = terrainTypes[0];
        for (int i = 1; i < terrainTypes.Length; i++)
        {
            if ((terrainTypes[i].heightCutoff > height && terrainTypes[i].heightCutoff < n.heightCutoff) ||
                (terrainTypes[i].heightCutoff > n.heightCutoff && n.heightCutoff < height))
                n = terrainTypes[i];
        }
        return n;
    }

    // Update is called once per frame
    void Update(){}
}
