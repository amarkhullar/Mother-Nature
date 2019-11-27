using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInit : MonoBehaviour {
	public GameObject canvas = null;
	public GameObject pointer = null;

	// Use this for initialization
	void Start () {
		canvas.SetActive(false);
		pointer.SetActive(false);
	}

}
