using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera Control Class.
/// This class is used to manage simple camera control within the game.
/// </summary>
public class CameraController : MonoBehaviour
{
    // Camera speed movement
    private float speed = 1.0f;
    // Zoom speed
    private float zoomSpeed = 7.0f;
    // Max Height Zoom
    private float zoomMax = 1000.0f;
    // Min Height Zoom
    private float zoomMin = 60.0f;

    // Update is called once per frames
    void Update()
    {
        // Camera movement

        // Forward
        if(Input.GetKey(KeyCode.Z))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + speed); 
        }
        // Backward
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - speed);
        }
        // Right
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }
        // Left
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = new Vector3(transform.position.x - speed, transform.position.y, transform.position.z);
        }

        //Mouse wheels

        // Zoom Forward
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
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
        // Zoom Backward
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
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
