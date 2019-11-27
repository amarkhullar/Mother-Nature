using Valve.VR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActionsTest : MonoBehaviour {

	public SteamVR_Input_Sources handType; // 1
	public SteamVR_Action_Boolean teleportAction; // 2a
	public SteamVR_Action_Boolean grabAction; // 3

	// Update is called once per frame
	void Update () {
		if (GetTeleportDown()){
    	print("Teleport " + handType);
		}

		if (GetGrab()){
    	print("Grab " + handType);
		}

	}

	public bool GetTeleportDown() { //1
  	return teleportAction.GetStateDown(handType);
	}

	public bool GetGrab() { //2
    return grabAction.GetState(handType);
	}

}
