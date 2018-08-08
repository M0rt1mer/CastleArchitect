using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyCamera : MonoBehaviour {

    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    public bool mouseLocked = true;
    
    float mainSpeed = 10.0f; //regular speed
    float shiftAdd = 25.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 100.0f; //Maximum speed when holdin gshift
    public float camSens = 0.25f; //How sensitive it with mouse
    private float totalRun = 1.0f;

    void Start() {
        GameObject.FindObjectOfType<Tooltip>().tooltipCallback.Add(getTooltip);
    }

    public IEnumerable<string> getTooltip() {
        List<string> lst = new List<string>();
        if (Input.GetAxis("Help") > 0) {
            if (!Input.GetButton("Move Camera"))
                lst.Add("Hold [Move Camera] to rotate camera");
            lst.Add("Hold [WSAD] to move camera");
        }
        return lst;
    }

    void Update() {

        mouseLocked = Input.GetButton("Move Camera") && ! Input.GetButton("LockDirection");
        Cursor.lockState =  mouseLocked?CursorLockMode.Locked:CursorLockMode.None;
        Cursor.visible = !mouseLocked;

        if (mouseLocked) { //if mouse locked, look around

            float upAngle = -Input.GetAxis("Mouse Y") * camSens + transform.eulerAngles.x;
            if (upAngle >= 80 && upAngle <= 280)
                upAngle = upAngle < 180 ? 80 : 320; // clamp*/
            //upAngle = Mathf.Clamp(upAngle, 10, 80);
            float sideAngle = Input.GetAxis("Mouse X") * camSens + transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(upAngle, sideAngle, 0);

        }
        
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift)) {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space)) { //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else {
            transform.Translate(p);
        }

    }

    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W)) {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S)) {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A)) {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}



