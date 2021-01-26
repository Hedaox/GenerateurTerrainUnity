using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage in Game controls.
/// </summary>
public class GameControl : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Press ESCAPE to show/hide menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject.Find("CanvasGameMenu").GetComponent<Canvas>().enabled ^= true;
        }
    }
}
