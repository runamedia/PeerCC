using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.WSA.Input;
using System;

public class GazeGestureManager : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }

    public GameObject ConnectButton;
    public GameObject CallButton;
    TextMesh textMeshConnect;
    TextMesh textMeshCall;
    // Use this for initialization
    void Start()
    {
        Instance = this;
        textMeshConnect = ConnectButton.GetComponentInChildren<TextMesh>();
        textMeshCall = CallButton.GetComponentInChildren<TextMesh>();
    }
    void Update()
    {
        if (textMeshConnect.color == Color.blue)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //ControlScript.Instance.OnConnectClick();
            }
        }
        if (textMeshCall.color == Color.blue)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //ControlScript.Instance.OnCallClick();
            }
        }
    } 
}
                