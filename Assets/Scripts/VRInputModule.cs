﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VRInputModule : BaseInputModule {

	public Camera m_Camera;
	public SteamVR_Input_Sources m_TargetSource;
	public SteamVR_Action_Boolean m_ClickAction;

	private GameObject m_CurrentObject = null;
	private PointerEventData m_Data = null;

	protected override void Awake(){
		base.Awake();
		m_Data = new PointerEventData(eventSystem);
	}

	// Behaves as update
	public override void Process(){
		// Reset data, set camera
		m_Data.Reset();
		m_Data.position = new Vector2(m_Camera.pixelWidth / 2, m_Camera.pixelHeight /2);

		// Raycast
		eventSystem.RaycastAll(m_Data, m_RaycastResultCache);
		m_Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
		m_CurrentObject = m_Data.pointerCurrentRaycast.gameObject;

		// Clear Raycast
		m_RaycastResultCache.Clear();

		// Handle Hover states
		HandlePointerExitAndEnter(m_Data, m_CurrentObject);

		// Press
		if(m_ClickAction.GetStateDown(m_TargetSource)){
			ProcessPress(m_Data);
		}

		// Release
		if(m_ClickAction.GetStateUp(m_TargetSource)){
			ProcessRelease(m_Data);
		}

	}

	public PointerEventData GetData(){
		return m_Data;
	}

	private void ProcessPress(PointerEventData data){
		// Set raycast for press
		data.pointerPressRaycast = data.pointerCurrentRaycast;

		// Check for object hit, get down handler, call
		GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(m_CurrentObject, data, ExecuteEvents.pointerDownHandler);

		// If no down handler, try get click handler
		if(newPointerPress == null){
			newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(m_CurrentObject);
		}

		// Set data
		data.pressPosition = data.position;
		data.pointerPress = newPointerPress;
		data.rawPointerPress = m_CurrentObject;
	}

	private void ProcessRelease(PointerEventData data){
		// Execute pointer up
		ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerDownHandler);

		// Check for click Handler
		GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(m_CurrentObject);

		// Check if actual
		if(data.pointerPress == pointerUpHandler){
			ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
		}

		// Clear selected gameObject
		eventSystem.SetSelectedGameObject(null);

		// Reset data
		data.pressPosition = Vector2.zero;
		data.pointerPress = null;
		data.rawPointerPress = null;

	}

}
