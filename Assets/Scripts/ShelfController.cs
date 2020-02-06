using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ShelfController : MonoBehaviour {
	public GameObject baseAbility;
	public GameObject shelf;

	private Vector3 scale = new Vector3(0.005f, 0.005f, 0.005f);

	private GameObject ability1;
	private GameObject ability2;
	private GameObject ability3;
	private Vector3 ability1Pos;
	private Vector3 ability2Pos;
	private Vector3 ability3Pos;


	void InitPositions(){
		ability1Pos = new Vector3(-0.02f, 0.005f, -0.002f);
		ability2Pos = new Vector3(-0.01f, 0.005f, -0.002f);
		ability3Pos = new Vector3(0.0f, 0.005f, -0.002f);
	}

	void InitAbility1(){
		ability1 = Instantiate(baseAbility);
		ability1.transform.parent = shelf.transform;
		ability1.transform.localPosition = ability1Pos;
		ability1.transform.localScale = scale;
	}

	void InitAbility2(){
		ability2 = Instantiate(baseAbility);
		ability2.transform.parent = shelf.transform;
		ability2.transform.localPosition = ability2Pos;
		ability2.transform.localScale = scale;
	}

	void InitAbility3(){
		ability3 = Instantiate(baseAbility);
		ability3.transform.parent = shelf.transform;
		ability3.transform.localPosition = ability3Pos;
		ability3.transform.localScale = scale;
	}

	// Use this for initialization
	void Start () {
		InitPositions();

		InitAbility1();
		InitAbility2();
		InitAbility3();

	}

	// Update is called once per frame
	void Update () {
		if (ability1 == null){
			InitAbility1();
		}
		if (ability2 == null){
			InitAbility2();
		}
		if (ability3 == null){
			InitAbility3();
		}
	}
}
