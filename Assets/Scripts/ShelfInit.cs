using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfInit : MonoBehaviour {
	public GameObject fireShelf = null;
	public GameObject waterShelf = null;
	public GameObject airShelf = null;
	public GameObject earthShelf = null;

	// Use this for initialization
	void Start () {
		fireShelf.SetActive(false);
		waterShelf.SetActive(false);
		airShelf.SetActive(false);
		earthShelf.SetActive(false);
	}
}
