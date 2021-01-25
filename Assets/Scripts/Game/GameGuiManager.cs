using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameGuiManager : MonoBehaviour
{
    // Game Menu
    public void OnClickGameMenuBackMainMenu()
    {
        SceneManager.LoadScene("Menu");
        // Reset Size Settings
        GameSettings.Size = 10;
    }
}