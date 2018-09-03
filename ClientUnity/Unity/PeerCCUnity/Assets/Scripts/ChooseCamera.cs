using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCamera : MonoBehaviour
{
    public Camera MainCam;
    public Camera Cam;

    void Update()
    {
        if (Input.GetKeyDown("q".ToLower()))
        {
            MainCam.gameObject.active = false;
            Cam.gameObject.active = true;
        }
        else if (Input.GetKeyDown("e".ToLower()))
        {
            MainCam.gameObject.active = true;
            Cam.gameObject.active = false;
        }
    }
}
