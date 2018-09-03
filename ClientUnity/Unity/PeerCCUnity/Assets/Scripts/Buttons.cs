using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Buttons : MonoBehaviour
{
    public GameObject ConnectButton;
    public GameObject CallButton;
    public GameObject FocusedObject { get; private set; }
    void Update()
    {
        if (FocusedObject != null)
        {
            if (FocusedObject.GetComponent<GameObject>() == ConnectButton)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    ControlScript.Instance.OnConnectClick();
                }
            }
            if (FocusedObject.GetComponent<GameObject>() == CallButton)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    ControlScript.Instance.OnCallClick();
                }
            }
        }

        GameObject oldFocusObject = FocusedObject;

        ////    // Do a raycast into the world based on the user's
        ////    // head position and orientation.
        RaycastHit hit;
        float distance;

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 1000;
        Debug.DrawRay(transform.position, forward, Color.red);

        if (Physics.Raycast(transform.position, forward, out hit))
        {
            FocusedObject = hit.collider.gameObject;
            distance = hit.distance;
        }
        else
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = new Vector2(transform.position.x, transform.position.y);
            List<RaycastResult> objects = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, objects);
            if (objects.Count > 0)
            {
                bool buttonGazed = false;
                foreach (RaycastResult result in objects)
                {
                    if (result.gameObject.GetComponent<GameObject>() == ConnectButton)
                    {
                        buttonGazed = true;
                        FocusedObject = result.gameObject;
                    }
                    else if (result.gameObject.GetComponent<GameObject>() == CallButton)
                    {
                        buttonGazed = true;
                        FocusedObject = result.gameObject;
                    }
                    System.Diagnostics.Debug.WriteLine(result.gameObject.name + " " + this.transform.position);
                }
                if (!buttonGazed)
                    FocusedObject = null;
            }
            else
            {
                // If the raycast did not hit a hologram, clear the focused object.
                FocusedObject = null;
            }
        }
    }
}
