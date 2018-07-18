using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class WorldCursor : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    public Button ConnectButton;
    public Button CallButton;
    public GameObject Cursor;
    private bool connectButtonSelected = false;
    private bool callButtonSelected = false;
    public float speed = 120f;
    public float sensitivity = 0.005f;
    CharacterController player;
    float moveUD;
    float moveLR;
    float rotX;
    float rotY;
    RaycastHit hit_info;
    
    // Use this for initialization
    void Start()
    {
        // Grab the mesh renderer that's on the same object as this script.
        meshRenderer = Cursor.gameObject.GetComponentInChildren<MeshRenderer>();

        player = GetComponent<CharacterController>();
    }
    // Update is called once per frame
    void Update()
    {
        moveUD = Input.GetAxis("Vertical") * speed;
        moveLR = Input.GetAxis("Horizontal") * speed;

        rotX = Input.GetAxis("Mouse X") * sensitivity;
        rotY = Input.GetAxis("Mouse Y") * sensitivity;

        Vector3 movement = new Vector3(moveLR, moveUD, 0);
        transform.Rotate(rotY, rotX, 0f);

        movement = transform.rotation * movement;
        player.Move(movement * Time.deltaTime);

        ////Do a raycast into the world based on the user's
        // //head position and orientation.
        var headPosition = transform.position;
        var gazeDirection = transform.forward;
        
        Ray ray = new Ray(headPosition, gazeDirection);
        Debug.DrawRay(headPosition, gazeDirection * 10, Color.red);
        if (Physics.Raycast(ray, out hit_info, 1000))
        {
            //    // If the raycast hit a hologram...
            //    // Display the cursor mesh.
            meshRenderer.enabled = true;
            //    // Move thecursor to the point where the raycast hit.
            Cursor.transform.position = ray.direction;

            //    // Rotate the cursor to hug the surface of the hologram.
            Cursor.transform.position = new Vector3(transform.position.x, transform.position.y, -40);
            //Cursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
        else
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = new Vector2(headPosition.x, headPosition.y);
            List<RaycastResult> objects = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, objects);
            if (objects.Count > 0)
            {
                bool buttonGazed = false;
                GameObject firstGameObject = null;
                System.Diagnostics.Debug.WriteLine("WorldCursor - RaycastAll objects hit");
                foreach (RaycastResult result in objects)
                {
                    if (firstGameObject == null)
                        firstGameObject = result.gameObject;
                    if (result.gameObject.GetComponent<Button>() == ConnectButton)
                    {
                        connectButtonSelected = true;
                        buttonGazed = true;
                        ColorBlock colorBlock = ConnectButton.colors;
                        colorBlock.normalColor = Color.red;
                        ConnectButton.colors = colorBlock;
                    }
                    else if (result.gameObject.GetComponent<Button>() == CallButton)
                    {
                        callButtonSelected = true;
                        buttonGazed = true;
                        ColorBlock colorBlock = CallButton.colors;
                        colorBlock.normalColor = Color.red;
                        CallButton.colors = colorBlock;
                    }
                }
                if (!buttonGazed)
                {
                    if (connectButtonSelected)
                    {
                        connectButtonSelected = false;
                        ColorBlock colorBlock = ConnectButton.colors;
                        colorBlock.normalColor = Color.white;
                        ConnectButton.colors = colorBlock;
                    }
                    if (callButtonSelected)
                    {
                        callButtonSelected = false;
                        ColorBlock colorBlock = CallButton.colors;
                        colorBlock.normalColor = Color.white;
                        CallButton.colors = colorBlock;
                    }
                }
                Cursor.transform.position = new Vector3(firstGameObject.transform.position.x, firstGameObject.transform.position.y, -40);
                meshRenderer.enabled = true;
            }
            else
            {
                // If the raycast did not hit a hologram, hide the cursor mesh.
                meshRenderer.enabled = false;
            }
        }
        }
    }