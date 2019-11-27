using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayShelf : MonoBehaviour{

	public void ShelfOn(){
		gameObject.SetActive(true);
	}

	public void ShelfOff(){
		gameObject.SetActive(false);
	}

  public void UpdateShelf(){
    gameObject.SetActive(!gameObject.activeSelf);
  }
}
