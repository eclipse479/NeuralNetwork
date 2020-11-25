using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manualCar : MonoBehaviour
{
    public float acceleration;
    public float maxSpeed;
    public float turnSpeed;
    public float maxTurnSpeed;

    private float velocity = 0;
    private float rotation = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotation -= turnSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotation += turnSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            velocity += acceleration * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            velocity -= acceleration * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
        transform.Rotate(new Vector3(0, 10, 0) * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            transform.Rotate(new Vector3(0, -10, 0) * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            transform.Rotate(new Vector3(0, 30, 0) * Time.deltaTime);
        }
        transform.Translate(new Vector3(0, 0, velocity) * Time.deltaTime);
    }
}
