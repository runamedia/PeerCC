using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondCamera : MonoBehaviour {

    Vector2 mouseLook;
    Vector2 smoothV;
    public float sensitivity = 5.0f;
    public float smoothing = 2.0f;
    GameObject character;
    bool click = false;
    public GameObject player;
    private Vector3 offset;

    void Start()
    {
        character = this.transform.parent.gameObject;
        offset = transform.position - player.transform.position;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            click = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            click = false;
        }
    }
    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;
        if (click)
        {
            var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
            smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
            mouseLook += smoothV;
            transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
        }
    }

}
