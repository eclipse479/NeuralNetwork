using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class walls : MonoBehaviour
{
    private GameObject[] theWalls;
    private bool isOn = true;
    // Start is called before the first frame update
    void Start()
    {
        theWalls = GameObject.FindGameObjectsWithTag("wall");
}

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            if (isOn)
            {
                foreach (GameObject entity in theWalls)
                {
                    Renderer rend = entity.GetComponent<Renderer>();
                    rend.enabled = false;
                }
                isOn = false;
            }
            else if (!isOn)
            {
                foreach (GameObject entity in theWalls)
                {
                    Renderer rend = entity.GetComponent<Renderer>();
                    rend.enabled = true;
                }
                isOn = true;
            }
        }
    }
}
