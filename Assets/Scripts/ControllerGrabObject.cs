using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerGrabObject : MonoBehaviour {

	public SteamVR_Input_Sources handType;
	public SteamVR_Behaviour_Pose controllerPose;
	public SteamVR_Action_Boolean grabAction;
	private GameObject collidingObject;
	private GameObject objectInHand;
	private GameObject abilityObject;
	private Vector3 abilityObjectStartPosition;

	private void SetCollidingObject(Collider col){

    	if (collidingObject || !col.GetComponent<Rigidbody>()){
        	return;
    	}

    	collidingObject = col.gameObject;
	}


	public void OnTriggerEnter(Collider other){
    	SetCollidingObject(other);
	}


	public void OnTriggerStay(Collider other){
    	SetCollidingObject(other);
	}


	public void OnTriggerExit(Collider other){
    	if (!collidingObject){
        	return;
    	}
    	collidingObject = null;
	}

	private void GrabObject(){
    	objectInHand = collidingObject;
    	collidingObject = null;
    	var joint = AddFixedJoint();
    	joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
	}

	private void GrabAbilityObject(){
		objectInHand = abilityObject;
    	collidingObject = null;
    	var joint = AddFixedJoint();
    	joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
	}

	private void InitAbilityObject(){
		abilityObject.GetComponent<Rigidbody>().useGravity = true;
		abilityObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		abilityObjectStartPosition = transform.position;
	}

	private FixedJoint AddFixedJoint(){
    FixedJoint fx = gameObject.AddComponent<FixedJoint>();
    fx.breakForce = 20000;
    fx.breakTorque = 20000;
    return fx;
	}

	private void ReleaseObject(){
		//Make sure there is a fixed joint attached to the controller
    if (GetComponent<FixedJoint>()){
				//Remove the connection and destroy the joint
        GetComponent<FixedJoint>().connectedBody = null;
        Destroy(GetComponent<FixedJoint>());
				//Add the speed and rotation of the controller when released
        objectInHand.GetComponent<Rigidbody>().velocity = controllerPose.GetVelocity();
        objectInHand.GetComponent<Rigidbody>().angularVelocity = controllerPose.GetAngularVelocity();

   	}
    objectInHand = null;
	}


	// Update is called once per frame
	void Update () {
		if (grabAction.GetLastStateDown(handType)){
    		if (collidingObject){
				AbilityObject script = collidingObject.GetComponent<AbilityObject>();
				if (script != null){
					if (true){
						//Cooldown complete, do shit
            abilityObject = collidingObject;
						InitAbilityObject();
						GrabAbilityObject();
					} else {
						//Ability in cooldown
					}
				} else {
					//Normal grab
					GrabObject();
				}
    		}
		}

		if (grabAction.GetLastStateUp(handType)){
    		if (objectInHand){
        		ReleaseObject();
    		}
		}
	}
}
