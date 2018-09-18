using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float sensitivity = 10f;
    public float maxYAngle = 80f;
    private Vector2 currentRotation;
    private bool IsRightClick = false;
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsRightClick = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            IsRightClick = false;
        }
    }
    private void LateUpdate()
    {
        if (IsRightClick)
        {
            currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
            currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
            currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
            currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
            transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }
}
