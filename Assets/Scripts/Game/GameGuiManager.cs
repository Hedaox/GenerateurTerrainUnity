using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manage GUI interface in game.
/// </summary>
public class GameGuiManager : MonoBehaviour
{
    /// <summary>
    /// Will display GAME MENU.
    /// </summary>
    public void OnClickGameMenuButton()
    {
        GameObject.Find("CanvasGameMenu").GetComponent<Canvas>().enabled ^= true;
        GameObject.Find("CanvasGUI").GetComponent<Canvas>().enabled ^= true;
    }

    /// <summary>
    /// Will go back to MAIN MENU.
    /// </summary>
    public void OnClickGameMenuBackMainMenu()
    {
        SceneManager.LoadScene("Menu");
        // Reset Size Settings
        GameSettings.Size = 10;
    }

    /// <summary>
    /// Will go back to MAIN MENU.
    /// </summary>
    public void OnClickGameBackGame()
    {
        GameObject.Find("CanvasGameMenu").GetComponent<Canvas>().enabled = false;
        GameObject.Find("CanvasGUI").GetComponent<Canvas>().enabled = true;
    }
}