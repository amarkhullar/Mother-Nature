using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CityPlayer : MonoBehaviour
{

    private GameObject selectedObject; // TODO: Check if this is necessary or if can skip and just use hextile
    private HexTile selectedTile;

    private Dictionary<ResourceTypeEnum, int> resources = new Dictionary<ResourceTypeEnum, int>();

    [SerializeField]
    private GameObject buildMenuObj;
    [SerializeField]
    private RectTransform uicanvas;
    [SerializeField]
    private BuildingList buildingList;

    void Update()
    {
        // TODO: Maybe move clicks to a IPointerClickHandler/IPointerDownHandler/IPointerUpHandler
        //       ^^ only bother if we end up with a lot of objects that are checking for clicks
        if(!IsPointerOverUIObject()) // Stops a raycast when over a button
        {

            // Left Click to select hextile
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Left click");
                Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 400))
                {
                    Debug.Log( hit.transform.gameObject.name);
                    Select(hit.transform.gameObject);
                }
            }

            // Right Click, deselects
            if(Input.GetMouseButtonDown(1))
            {
                Deselect();
            }
        }
    }

    public void Select(GameObject go)
    {
        Deselect();

        selectedObject = go;
        selectedTile = selectedObject.GetComponent<HexTile>(); // TODO: Ensure this will always work, even if there are other objects with collider meshes

        // TODO: Change this to some sort of highlighting affect.
        selectedTile.SetMaterialColor(Color.red);

        ShowBuildMenu();
    }

    public void Deselect()
    {
        if(selectedTile != null) selectedTile.ResetMaterialColor();
        HideBuildMenu();
    }

    private bool menuOpen = false;
    private GameObject menu;
    void ShowBuildMenu()
    {
        // TODO: Decide whether this appears at a static location on the screen, or if it appears in relation to the tile
        // Maybe this is completely different and always have a menu of buildings, from which we drag a building onto a tile w/o bothering to select it.
        // TODO: Show proper menu

        if(menuOpen) return;
        menuOpen = true;
        menu = Instantiate(buildMenuObj);

        menu.transform.SetParent(uicanvas.transform, false);
        menu.transform.localScale = new Vector3(1, 1, 1);

        menu.GetComponent<BuildingMenu>().GenerateButtons(buildingList, this);

        //RectTransform rt = menu.GetComponent<RectTransform>();
        // rt.anchoredPosition.Set(-500, -200);

        // This can be set on the buttons themselves once a proper menu has been made.
        // Button button = menu.GetComponent<Button>();
        // button.onClick.AddListener(() => onBuildButtonPress());
    }

    void HideBuildMenu(){
        if(!menuOpen) return;
        Destroy(menu);
        menuOpen = false;
    }

    public void OnBuildButtonPress(GameObject building)
    {
        // TODO: Check if we have resources required.
        // TODO: Reduce resources based on building cost.

        // PlaceBuilding() checks the validity of the building
        selectedTile.PlaceBuilding(building);
        Building b = building.GetComponent<Building>();
        b.owner = this;
    }


    // Maybe move this to a more suitable generic location? When Touching UI
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
