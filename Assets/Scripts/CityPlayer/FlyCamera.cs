using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour {

    /*
    wasd : basic movement
    shift : Makes camera accelerate*/


    [SerializeField]
    float mainSpeed = 100.0f; //regular speed
    [SerializeField]
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    [SerializeField]
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
//    float camSens = 0.25f; //How sensitive it with mouse
//    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun= 1.0f;

    public float dragSpeed = 1000f;
    public float minFov = 10;
    public float maxFov = 100;
    public float defaultFov = 60;
    public float zoomSpeed = 10;
    private float curFov;
    private Vector3 dragOrigin;
    private Vector3 worldOrigin;
    private Camera cam;

    void Start(){
        cam = GetComponent<Camera>();
        cam.fieldOfView = defaultFov;
        curFov = defaultFov;
    }

    void Update () {

        //Keyboard commands
        Vector3 p = GetBaseInput();
        if (Input.GetKey (KeyCode.LeftShift)){
            totalRun += Time.deltaTime;
            p  = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else{
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;

        Vector3 newPosition = transform.position;
        transform.Translate(p);
        newPosition.x = transform.position.x;
        newPosition.z = transform.position.z;
        transform.position = newPosition;

        UpdateZoom();

        // Camera dragging with Right mouse button.
        if (Input.GetMouseButtonDown(1))
        {
            worldOrigin = transform.position;
            dragOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            dragOrigin.z = dragOrigin.y;
            return;
        }

        if (!Input.GetMouseButton(1)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        pos.z = pos.y;
        Vector3 end = worldOrigin + (dragOrigin - pos) * dragSpeed;
        end.y = transform.position.y;
        transform.position = end;

    }

    private void UpdateZoom(){
        // Zoom (changing FoV)
        if(Input.GetKey(KeyCode.E))
            curFov -= zoomSpeed * Time.deltaTime;
        else if(Input.GetKey(KeyCode.Q))
            curFov += zoomSpeed * Time.deltaTime;

        if(curFov < minFov) curFov = minFov;
        if(curFov > maxFov) curFov = maxFov;

        cam.fieldOfView = curFov;
    }

    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}
