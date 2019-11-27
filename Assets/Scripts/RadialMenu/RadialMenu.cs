using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour{
  [Header("Scene")]
  public Transform selectionTransform = null;
  public Transform cursorTransform = null;
  public GameObject radialUp = null;
  public GameObject radialRight = null;
  public GameObject radialDown = null;
  public GameObject radialLeft = null;

  [Header("Events")]
  public RadialSection top = null;
  public RadialSection right = null;
  public RadialSection bottom = null;
  public RadialSection left = null;

  private Vector2 touchPosition = Vector2.zero;
  private List<RadialSection> radialSections = null;
  private RadialSection highlightedSection = null;

  private readonly float degreeIncrement = 90.0f;

  private void Awake(){
    CreateAndSetupSections();
  }

  private void CreateAndSetupSections(){
    radialSections = new List<RadialSection>(){
      top,
      right,
      bottom,
      left
    };

    //foreach(RadialSection section in radialSections)
      //section.iconRenderer.sprite = section.icon;
  }

  private void Start(){
    Show(false);
    radialUp.SetActive(false);
    radialRight.SetActive(false);
    radialDown.SetActive(false);
    radialLeft.SetActive(false);
  }

  public void Show(bool value){
    gameObject.SetActive(value);
  }

  private void Update(){
    Vector2 direction = Vector2.zero + touchPosition;
    float rotation = GetDegree(direction);

    SetCursorPosition();
    SetSelectionRotation(rotation);
    SetSelectedEvent(rotation);
  }

  private float GetDegree(Vector2 direction){
    float value = Mathf.Atan2(direction.x, direction.y);
    value *= Mathf.Rad2Deg;
    if(value < 0){
      value += 360.0f;
    }
    return value;
  }

  private void SetCursorPosition(){
    cursorTransform.localPosition = touchPosition;
    //print(touchPosition);
  }

  private void SetSelectionRotation(float newRotation){
    float snappedRotation = SnapRotation(newRotation);
    selectionTransform.localEulerAngles = new Vector3(0, 0, -snappedRotation);
  }

  private float SnapRotation(float rotation){
    return GetNearestIncrement(rotation) * degreeIncrement;
  }

  private int GetNearestIncrement(float rotation){
    return Mathf.RoundToInt(rotation / degreeIncrement);
  }

  private void SetSelectedEvent(float currentRotation){
    int index = GetNearestIncrement(currentRotation);
    if (index == 4) index = 0;
    highlightedSection = radialSections[index];
    SetRadialSectionActive(index);
  }

  private void SetRadialSectionActive(int index){
    if (index == 0){
      radialUp.SetActive(true);
      radialRight.SetActive(false);
      radialDown.SetActive(false);
      radialLeft.SetActive(false);
    } else if (index == 1){
      radialUp.SetActive(false);
      radialRight.SetActive(true);
      radialDown.SetActive(false);
      radialLeft.SetActive(false);
    } else if (index == 2){
      radialUp.SetActive(false);
      radialRight.SetActive(false);
      radialDown.SetActive(true);
      radialLeft.SetActive(false);
    } else if (index == 3){
      radialUp.SetActive(false);
      radialRight.SetActive(false);
      radialDown.SetActive(false);
      radialLeft.SetActive(true);
    }
  }

  public void SetTouchPosition(Vector2 newValue){
    touchPosition = newValue;
  }

  public void ActivateHighlightedSection(){
    highlightedSection.onPress.Invoke();
  }

}
