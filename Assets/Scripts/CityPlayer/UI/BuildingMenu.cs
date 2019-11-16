using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenu : MonoBehaviour
{
    // This could all be put in CityPlayer if needed to ig, but probs easier & cleaner to keep stuff as separated as possible separated.

    [SerializeField]
    public GameObject button; // TODO: Change these buttons, maybe they should show an image of the building?

    public void GenerateButtons(BuildingList bl, CityPlayer player)
    {
        // TODO: Check if player has resources required to build building. If not, grey out the button.
        // How often should menu be rechecked for what is valid?
        List<GameObject> bls = bl.buildings;

        for(int i = 0; i < bls.Count; i++)
        {
            GameObject b = bls[i];

            GameObject butObj = Instantiate(button);
            butObj.transform.SetParent(GetComponent<Transform>(), false);
            butObj.transform.Translate(-Screen.width/3f - 50 + 200 * i, - Screen.height/3f - 50, 0);  // TODO: find a better way to poition UI elements

            Button bttn = butObj.GetComponent<Button>();
            bttn.onClick.AddListener(() => player.OnBuildButtonPress(b));
            butObj.GetComponentInChildren<Text>().text = "Place: " + b.transform.name;
        }
    }

}
