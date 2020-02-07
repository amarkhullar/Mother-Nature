using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAbility : MonoBehaviour {

	public ShelfController shelfController;
	private int shelfIndex;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision col){
		shelfController.CollisionDetected(this, col);
	}


	public void setShelfIndex(int index) {
		shelfIndex = index;
	}

	public int getShelfIndex() {
		return shelfIndex;
	}
}
