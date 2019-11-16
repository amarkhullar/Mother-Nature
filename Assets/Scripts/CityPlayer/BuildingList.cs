using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> buildings = new List<GameObject>();

    // Maybe at some point it'll be easier to programmatically generate the building list from a text file, not there yet tho.
    // Maybe this should also be a singleton or something? No need to ever duplicate this
}
