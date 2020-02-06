using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class LaserPointer : MonoBehaviour {

	public SteamVR_Input_Sources handType;
	public SteamVR_Behaviour_Pose controllerPose;
	public SteamVR_Action_Boolean teleportAction;
	public GameObject laserPrefab;
	private GameObject laser;
	private Transform laserTransform;
	private Vector3 hitPoint;

	// Cloud Planes
	public GameObject cloud;
	private Collider cloudCollider;
	public GameObject cloudPlane;
	private Collider cloudPlaneCollider;

	public Transform cameraRigTransform;
	public GameObject teleportReticlePrefab;
	private GameObject reticle;
	private Transform teleportReticleTransform;
	public Transform headTransform;
	public Vector3 teleportReticleOffset;
	public LayerMask teleportMask;
	private bool shouldTeleport;

	// Use this for initialization
	void Start () {
		// collider components to toggle
		cloudCollider = cloud.GetComponent<Collider>();
		cloudPlaneCollider = cloudPlane.GetComponent<Collider>();

		laser = Instantiate(laserPrefab);
		laserTransform = laser.transform;
		reticle = Instantiate(teleportReticlePrefab);
		teleportReticleTransform = reticle.transform;
	}

	// Update is called once per frame
	void Update () {
		if (teleportAction.GetState(handType)){
    	RaycastHit hit;

			// enable collider when trigger is pressed
			cloudCollider.enabled = true;
			cloudPlaneCollider.enabled = true;

			if (Physics.Raycast(controllerPose.transform.position, transform.forward, out hit, 100, teleportMask)){
				 	hitPoint = hit.point;
        	ShowLaser(hit);
					reticle.SetActive(true);
					teleportReticleTransform.position = hitPoint + teleportReticleOffset;
					shouldTeleport = true;
    		}
		} else {
    	laser.SetActive(false);
			reticle.SetActive(false);
		}
		if (teleportAction.GetStateUp(handType) && shouldTeleport){
    	Teleport();

			// disable collider when trigger is released
			cloudCollider.enabled = false;
			cloudPlaneCollider.enabled = false;
		}
	}

	private void ShowLaser(RaycastHit hit){
    laser.SetActive(true);
    laserTransform.position = Vector3.Lerp(controllerPose.transform.position, hitPoint, .5f);
    laserTransform.LookAt(hitPoint);
    laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
	}

	private void Teleport(){
    shouldTeleport = false;
    reticle.SetActive(false);
    Vector3 difference = cameraRigTransform.position - headTransform.position;
    difference.y = 0;
    cameraRigTransform.position = hitPoint + difference;
	}
}
