using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Sprite sprite;
    Dictionary<string, GameObject> gameObjectList;
    List<GameObject> cubeList;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        gameObjectList = new Dictionary<string, GameObject>();
        cubeList = new List<GameObject>();
    }
    void Update ()
    {
        if (Input.GetKeyDown("k"))
        {
            string panelName = "test";
            gameObjectList.Add(panelName, new GameObject(panelName));
            gameObjectList[panelName].AddComponent<SpriteRenderer>();
            spriteRenderer = gameObjectList[panelName].GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            cubeList.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            gameObjectList[panelName].transform.parent = cubeList[0].transform;
            cubeList[0].name = panelName;
            cubeList[0].transform.localScale = new Vector3(8, 6, 3);
            cubeList[0].transform.position = new Vector3(0, 0, 0);
            gameObjectList[panelName].transform.localScale = new Vector3(0.04f, 0.14f, 0.51f);
        }
    }
    
}
