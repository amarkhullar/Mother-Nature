using System;
using UnityEngine;

public class GeneratePlanes : MonoBehaviour
{
    [SerializeField]
    public GameObject plane;

    [SerializeField]
    public int x, z;

    void Start()
    {
        for(int i = 0; i < x; i++)
        {
            for(int j = 0; j < z; j++)
            {

                Instantiate(plane, new Vector3(i * 10, 0, j * 10), this.transform.rotation);
            }
        }
    }
}

