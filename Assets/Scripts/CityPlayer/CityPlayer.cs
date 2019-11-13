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
    private GameObject building;

    void Update()
    {
        if(!IsPointerOverUIObject())
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

    }

    private bool menuOpen = false;
    void ShowBuildMenu()
    {
        // TODO: Decide whether this appears at a static location on the screen, or if it appears in relation to the tile
        // Maybe this is completely different and always have a menu of buildings, from which we drag a building onto a tile w/o bothering to select it.
        // TODO: Show proper menu

        if(menuOpen) return;
        menuOpen = true;
        GameObject menu = Instantiate(buildMenuObj);

        menu.transform.SetParent(uicanvas.transform, false);
        menu.transform.localScale = new Vector3(1, 1, 1);

        RectTransform rt = menu.GetComponent<RectTransform>();
        rt.anchoredPosition.Set(-500, -200);

        // This can be set on the buttons themselves once a proper menu has been made.
        Button button = menu.GetComponent<Button>();
        button.onClick.AddListener(() => tempBuild());
    }

    public void tempBuild()
    {
        // TODO: Change this to take in a building.
        // TODO: Reduce resources based on building cost.

        selectedTile.PlaceBuilding(building);
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
