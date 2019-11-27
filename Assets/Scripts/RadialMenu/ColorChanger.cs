using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour{
  public Material waterMaterial;
  public Material fireMaterial;
  public Material airMaterial;
  public Material earthMaterial;

  public void SetWater(){
    SetMaterial(waterMaterial);
  }

  public void SetFire(){
    SetMaterial(fireMaterial);
  }

  public void SetAir(){
    SetMaterial(airMaterial);
  }

  public void SetEarth(){
    SetMaterial(earthMaterial);
  }

  private void SetMaterial(Material newMaterial){
    gameObject.GetComponent<Renderer>().material = newMaterial;
  }
}
