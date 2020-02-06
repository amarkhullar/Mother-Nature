using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObject : MonoBehaviour {
	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision col){
		GameObject tile = col.gameObject;
		Material mat = tile.GetComponent<MeshRenderer>().material;
		mat.color = Color.red;
		Destroy(gameObject);
	}
}
