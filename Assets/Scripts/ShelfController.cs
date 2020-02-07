using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ShelfController : MonoBehaviour {
	public BaseAbility baseAbility;
	public GameObject shelf;

	private static int numOfAbilities = 5;
	private BaseAbility[] abilities = new BaseAbility[numOfAbilities];

	private static Vector3 basePosition = new Vector3(-0.02f, 0.005f, -0.002f);
	private static Vector3 incPosition = new Vector3(0.01f, 0.0f, 0.0f);
	private static Vector3 scale = new Vector3(0.005f, 0.005f, 0.005f);

	// private bool timerActive = true;

	void Start () {
		for (int i = 0; i < numOfAbilities; i++) {
			ActivateAbility(i);
		}
	}

	void ActivateAbility(int pos) {

		if (abilities[pos] == null) {
			abilities[pos] = Instantiate<BaseAbility>(baseAbility);
			abilities[pos].transform.parent = shelf.transform;
			abilities[pos].transform.localPosition = basePosition + (pos * incPosition);
			abilities[pos].transform.localScale = scale;
			abilities[pos].setShelfIndex(pos);
		}
	}

	void DeactivateAbility(BaseAbility ability) {
		Destroy(ability.gameObject);
	}

	IEnumerator CooldownAbility(BaseAbility ability) {
		DeactivateAbility(ability);
		yield return new WaitForSeconds(2);
		ActivateAbility(ability.getShelfIndex());
	}

	public void CollisionDetected(BaseAbility ability, Collision col) {
		Debug.Log(col.collider.name);

		if (col.collider.name == ("HexagonalTile(Clone)")) {
			GameObject obj = col.gameObject;
			obj.GetComponent<MeshRenderer>().material.color = Color.red;
			StartCoroutine(CooldownAbility(ability));
		}
		else {
			//DeactivateAbility(ability);
		}
	}


	// Update is called once per frame
	void Update () {

	}
}
