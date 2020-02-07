using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ShelfController : MonoBehaviour {
	public BaseAbility baseAbility;
	public GameObject shelf;

	private static int NUM_PLANES = 10;
	private BaseAbility[] abilities = new BaseAbility[NUM_PLANES];

	private static Vector3 firstShelfBasePos = new Vector3(-0.02f, 0.005f, -0.002f);
	private static Vector3 incPosition = new Vector3(0.01f, 0.0f, 0.0f);
	private static Vector3 scale = new Vector3(0.005f, 0.005f, 0.005f);

	// private bool timerActive = true;

	void Start () {
		for (int i = 0; i < NUM_PLANES; i++) {
			ActivateAbility(i);
		}
	}

	void ActivateAbility(int pos) {

		if (abilities[pos] == null) {

			abilities[pos] = Instantiate<BaseAbility>(baseAbility);
			abilities[pos].transform.parent = shelf.transform;
			abilities[pos].transform.localPosition = firstShelfBasePos + (pos * incPosition);

			// Second row
			if (pos >= 5){
				abilities[pos].transform.localPosition += new Vector3(-0.05f, -0.01f, 0.0f);
			}

			abilities[pos].transform.localScale = scale;
			abilities[pos].setShelfIndex(pos);
			abilities[pos].GetComponent<MeshRenderer>().material.color = shelf.GetComponent<MeshRenderer>().material.color;
		}
	}

	void DeactivateAbility(BaseAbility ability) {
		Destroy(ability.gameObject);
	}

	IEnumerator CooldownAbility(BaseAbility ability) {
		int shelfIndex = ability.getShelfIndex();

		DeactivateAbility(ability);
		yield return new WaitForSeconds(shelfIndex);
		ActivateAbility(shelfIndex);

		//Debug.Log("respawned");
	}

	public void CollisionDetected(BaseAbility ability, Collision col) {
		Debug.Log(col.collider.name);

		if (col.collider.name == ("HexagonalTile(Clone)")) {
			GameObject obj = col.gameObject;
			obj.GetComponent<MeshRenderer>().material.color = ability.GetComponent<MeshRenderer>().material.color;
			Destroy(obj.GetComponent<HexTile>().top);
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
