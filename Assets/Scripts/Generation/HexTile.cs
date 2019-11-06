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
    public GameObject top;
    public TerrainType terrain;
    int terrainVariation;
    public bool isAlive = true; // TODO Come up with better name.

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
        Vector3 center = transform.localPosition;
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center,
                center + HexTileMetrics.corners[i],
                center + HexTileMetrics.corners[i + 1]
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
                SetObjectOnTile(t.spawnableResource[var]);
            }
    }

    public void SetObjectOnTile(GameObject go)
    {
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

}
