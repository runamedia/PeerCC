using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptForMove : MonoBehaviour
{
    private void Update()
    {
        var moveX = Input.GetAxis("Horizontal") * 1000 * Time.deltaTime;
        var moveZ = Input.GetAxis("Vertical") * 1000 * Time.deltaTime;
        transform.Translate(moveX, 0, moveZ);
    }
}
