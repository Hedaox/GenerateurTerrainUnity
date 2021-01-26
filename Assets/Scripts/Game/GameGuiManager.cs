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
    /// Will go back to MAIN MENU.
    /// </summary>
    public void OnClickGameMenuBackMainMenu()
    {
        SceneManager.LoadScene("Menu");
        // Reset Size Settings
        GameSettings.Size = 10;
    }
}