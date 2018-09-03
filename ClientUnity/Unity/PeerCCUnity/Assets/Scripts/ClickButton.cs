using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnConnectClick()
    {
        ControlScript.Instance.OnConnectClick();    
    }

    public void OnCallClick()
    {
        ControlScript.Instance.OnCallClick();
    }
}
