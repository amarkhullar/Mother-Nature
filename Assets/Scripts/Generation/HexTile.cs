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
    public double resourceAmount;

    // FOR DEBUG ONLY
    public float heat;
    public float height;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        GetComponent<MeshCollider>().sharedMesh = mesh;
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

        // NB: Probs able to change the *scale's by updating HexTileMetrics, if performance gains are needed.
        Vector3 center = transform.localPosition;
        AddTriangle(
            center + HexTileMetrics.corners[0],
            center + HexTileMetrics.corners[1],
            center + HexTileMetrics.corners[2]
        );
        AddTriangle(
            center + HexTileMetrics.corners[0],
            center + HexTileMetrics.corners[2],
            center + HexTileMetrics.corners[3]
        );
        AddTriangle(
            center + HexTileMetrics.corners[0],
            center + HexTileMetrics.corners[3],
            center + HexTileMetrics.corners[5]
        );
        AddTriangle(
            center + HexTileMetrics.corners[3],
            center + HexTileMetrics.corners[4],
            center + HexTileMetrics.corners[5]
        );

        // Sides
        for(int i = 0; i < 6; i++)
        {
            Vector3 lowa = new Vector3(HexTileMetrics.corners[i].x, HexTileMetrics.corners[i].y-HexTileMetrics.height, HexTileMetrics.corners[i].z);
            Vector3 lowb = new Vector3(HexTileMetrics.corners[i+1].x, HexTileMetrics.corners[i+1].y-HexTileMetrics.height, HexTileMetrics.corners[i+1].z);
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
        terrainVariation = var;
        SetMaterialColor(t.color[var]);
        /*
        if (t.spawnableResource.Length > var)
            if(t.spawnableResource[var] != null)
            {
                GameObject go = t.spawnableResource[var];
                SetObjectOnTile(go);
                resourceType = go.GetComponent<ResourceObject>().resourceType;
            }
            */
    }

    // Should not be used outside of generation.
    public void SetResourceOnTile(GameObject go){
        SetObjectOnTile(go);
        if(go.GetComponent<ResourceObject>() == null)
        {
            Debug.LogWarning("Should this object have a resource/building script?");
            return;
        }

        resourceType = go.GetComponent<ResourceObject>().resourceType;
        resourceAmount = 100;
    }

    public GameObject SetObjectOnTile(GameObject go)
    {
        if(go.GetComponent<Building>() == null && go.GetComponent<ResourceObject>() == null) Debug.LogWarning("Should this object have a resource/building script?");

        Destroy(top);
        top = Instantiate(go);
        top.transform.SetParent(gameObject.transform, false);
        top.transform.localPosition = gameObject.transform.localPosition;

        return top;
    }

    public void ResetMaterialColor()
    {
        Color c = terrain.color[terrainVariation];
        SetMaterialColor(c);
        prev = c;
    }

    public void SetMaterialColor(Color c)
    {
        Material mat = GetComponent<MeshRenderer>().material;
        mat.color = c;
    }

    //////// END TILE GENERATION ////////

    public bool PlaceBuilding(GameObject go, CityPlayer owner)
    {
        Building bscript = go.GetComponent<Building>();
        if(bscript != null && ((bscript.placedOnWater && this.terrain.name.Equals("Water")) || (!this.terrain.name.Equals("Water"))))
        {
            GameObject g2 = SetObjectOnTile(go);
            Building bscript2 = g2.GetComponent<Building>();
            bscript2.owner = owner;
            bscript2.tile = this;
            bscript2.location = new Vector2(x,z);
            return true;
        }
        else Debug.LogWarning("Cannot place a non building as a building. (" + go.name + ")");
        return false;
    }

    // Tile Highlighting:
    // TODO: See if this works better on a decent pc when compiled, a bit behind for me rn.
    public Color prev; // public to allow for a tad bit of a hack from CityPlayer.cs - should find a better way to do this.
    public void OnMouseEnter(){
        if(!SceneVariables.vrscene)
        {
            prev = GetComponent<MeshRenderer>().material.color;
            SetMaterialColor(Color.blue);
        }
    }

    public void OnMouseExit(){
        if(!SceneVariables.vrscene)
            SetMaterialColor(prev);
    }

    public Vector2 GetLocation()
    {
        return new Vector2(x, z);
    }

}
