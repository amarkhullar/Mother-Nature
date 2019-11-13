using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexTile : MonoBehaviour
{

    // TODO:
    //      Tile Health
    //      Tile Productivity
    //      Set Tile's layer to something so it can collide with VRPlayer's attacks.

    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    public int x, z;
    public TerrainType terrain;
    public int terrainVariation;
    public bool isAlive = true; // TODO Come up with better name.
    public GameObject top; // Displayed Object
    public ResourceTypeEnum resourceType = ResourceTypeEnum.NONE; // Could be changed to ResourceObject, then inherit the script from the tree/rock placed on top.
    // Maybe some value of how much is left? Could be put in ResourceObject if doing ^^^.

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    //////// TILE GENERATION ////////

    public void GenerateMesh()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();

        // Top Hexagonal plate
        Vector3 center = transform.localPosition;
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center,
                center + HexTileMetrics.corners[i],
                center + HexTileMetrics.corners[i + 1]
            );
        }

        int sideHeight = 20;
        // Sides
        for(int i = 0; i < 6; i++)
        {
            Vector3 lowa = new Vector3(HexTileMetrics.corners[i].x, HexTileMetrics.corners[i].y-sideHeight, HexTileMetrics.corners[i].z);
            Vector3 lowb = new Vector3(HexTileMetrics.corners[i+1].x, HexTileMetrics.corners[i+1].y-sideHeight, HexTileMetrics.corners[i+1].z);
            AddTriangle(
                center + HexTileMetrics.corners[i],
                center + lowa,
                center + lowb
            );
            AddTriangle(
                center + HexTileMetrics.corners[i+1],
                center + HexTileMetrics.corners[i],
                center + lowb
            );
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    public void SetTerrainType((TerrainType, int) t)
    {
        SetTerrainType(t.Item1, t.Item2);
    }

    public void SetTerrainType(TerrainType t, int var)
    {
        terrain = t;
        SetMaterialColor(t.color[var]);

        if (t.spawnableResource.Length > var)
            if(t.spawnableResource[var] != null)
            {
                GameObject go = t.spawnableResource[var];
                SetObjectOnTile(go);
                resourceType = go.GetComponent<ResourceObject>().resourceType;
            }

    }

    public void SetObjectOnTile(GameObject go)
    {
        if(go.GetComponent<Building>() == null && go.GetComponent<ResourceObject>() == null) Debug.LogWarning("Should this object have a resource/building script?");

        Destroy(top);
        top = Instantiate(go, transform.position, Quaternion.identity);
        top.transform.SetParent(transform, false);
        top.transform.Translate(0, 0.5f, 0);
    }

    void SetMaterialColor(Color c)
    {
        Material mat = GetComponent<MeshRenderer>().material;
        mat.color = c;
    }

    //////// END TILE GENERATION ////////

    void PlaceBuilding(GameObject go)
    {
        Building bscript = go.GetComponent<Building>();
        if(bscript != null)
        {
            SetObjectOnTile(go);
            // TODO
        }
        Debug.LogWarning("Cannot place a non building as a building.");
    }

}
