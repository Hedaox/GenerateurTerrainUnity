using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private float speed = 1.0f;
    private float zoomSpeed = 7.0f;
    private float zoomMax = 1000.0f;
    private float zoomMin = 60.0f;

    // Update is called once per frame
    void Update()
    {
        // Camera movement

        if(Input.GetKey(KeyCode.Z))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + speed); 
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = new Vector3(transform.position.x - speed, transform.position.y, transform.position.z);
        }

        //Mouse wheels

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            if(GetComponent<Camera>().orthographicSize > zoomMin)
            {
                GetComponent<Camera>().orthographicSize -= zoomSpeed;
            }
            else
            {
                GetComponent<Camera>().orthographicSize = zoomMin;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            if (GetComponent<Camera>().orthographicSize < zoomMax)
            {
                GetComponent<Camera>().orthographicSize += zoomSpeed;
            }
            else
            {
                GetComponent<Camera>().orthographicSize = zoomMax;
            }
        }
    }
}
