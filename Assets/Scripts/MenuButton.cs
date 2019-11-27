using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MenuButton : MonoBehaviour {

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean menuAction;
	public bool showMenu;
	public GameObject menuPointer;
  public GameObject menuCanvas;

	// Use this for initialization
	void Start () {
		showMenu = true;
	}

	// Update is called once per frame
	void Update () {
		if (GetMenuButtonDown()){
			if (showMenu == false){
				showMenu = true;
				menuPointer.SetActive(true);
				menuCanvas.SetActive(true);
			} else {
				showMenu = false;
				menuPointer.SetActive(false);
				menuCanvas.SetActive(false);
			}
    	print("Menu button pressed, showMenu: " + showMenu);
		}

	}

	public bool GetMenuButtonDown() { //1
  	return menuAction.GetStateDown(handType);
	}

	private void DisplayMenu(){
		menuCanvas.transform.position.Set(0,0,10);
	}

	private void HideMenu(){
		menuCanvas.transform.position.Set(10000,0,10000);
	}

}
