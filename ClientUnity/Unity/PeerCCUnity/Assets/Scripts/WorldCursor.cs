using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class WorldCursor : MonoBehaviour
{
    public GameObject ConnectButton;
    public GameObject CallButton;
    public Camera MainCamera;
    private bool connectButtonSelected = false;
    private bool callButtonSelected = false;
    public float speed = 120f;
    public float sensitivity = 0.005f;
    RaycastHit hit_info;
    TextMesh textMeshConnect;
    TextMesh textMeshCall;

    // Use this for initialization
    void Start()
    {
        textMeshConnect = ConnectButton.GetComponentInChildren<TextMesh>();
        textMeshCall = CallButton.GetComponentInChildren<TextMesh>();
    }
    
    void Update()
    {
       
        var headPosition = MainCamera.transform.position;
        var gazeDirection = MainCamera.transform.forward * 10;

        Debug.DrawRay(headPosition, gazeDirection, Color.red);

        this.transform.position = new Vector3(gazeDirection.x + headPosition.x, gazeDirection.y + headPosition.y, -0.5f);
        
        if (Physics.Raycast(headPosition, gazeDirection, out hit_info, 1000))
        {
            this.transform.localScale = new Vector3(0.07f, 0.07f, 1);
            if (hit_info.collider.gameObject.name == "ConnectButton")
            {
                textMeshConnect.color = Color.blue;
            }
            else
            {
                textMeshConnect.color = Color.white;
            }
            if (hit_info.collider.gameObject.name == "CallButton")
            {
                textMeshCall.color = Color.blue;
            }
            else
            {
                textMeshCall.color = Color.white;
            }

        }
        else
        {
            this.transform.localScale = new Vector3(0.1f, 0.1f, 1);
            textMeshConnect.color = Color.white;
            textMeshCall.color = Color.white;
            //PointerEventData pointerData = new PointerEventData(EventSystem.current);
            //pointerData.position = new Vector2(gazeDirection.x, gazeDirection.y);
            //List<RaycastResult> objects = new List<RaycastResult>();
            //EventSystem.current.RaycastAll(pointerData, objects);
            //if (objects.Count > 0)
            //{
            //    bool buttonGazed = false;
            //    GameObject firstGameObject = null;
            //    System.Diagnostics.Debug.WriteLine("WorldCursor - RaycastAll objects hit");
            //    foreach (RaycastResult result in objects)
            //    {
            //        if (firstGameObject == null)
            //            firstGameObject = result.gameObject;
            //        if (result.gameObject.GetComponent<Button>() == ConnectButton)
            //        {
            //            //connectButtonSelected = true;
            //            //buttonGazed = true;
            //            //ColorBlock colorBlock = ConnectButton.GetComponentInChildren<TextMesh>().color;
            //            //colorBlock.normalColor = Color.red;
            //            //ConnectButton.colors = colorBlock;
            //        }
            //        else if (result.gameObject.GetComponent<Button>() == CallButton)
            //        {
            //            //callButtonSelected = true;
            //            //buttonGazed = true;
            //            //ColorBlock colorBlock = CallButton.colors;
            //            //colorBlock.normalColor = Color.red;
            //            //CallButton.colors = colorBlock;
            //        }
            //    }
            //    if (!buttonGazed)
            //    {
            //        if (connectButtonSelected)
            //        {
            //            //connectButtonSelected = false;
            //            //ColorBlock colorBlock = ConnectButton.colors;
            //            //colorBlock.normalColor = Color.white;
            //            //ConnectButton.colors = colorBlock;
            //        }
            //        if (callButtonSelected)
            //        {
            //            //callButtonSelected = false;
            //            //ColorBlock colorBlock = CallButton.colors;
            //            //colorBlock.normalColor = Color.white;
            //            //CallButton.colors = colorBlock;
            //        }
            //    }
            //    transform.position = new Vector3(firstGameObject.transform.position.x, firstGameObject.transform.position.y, -0.5f);
            //}
        }
    }
}