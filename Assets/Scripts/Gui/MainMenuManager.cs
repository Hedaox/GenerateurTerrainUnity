using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CotcSdk;
using Newtonsoft.Json.Linq;
using System.IO;

/// <summary>
/// Manage Main Menu interface Action
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // Loading progress when starting a game
    private static float loadingProgress = 0.0f;

    // Used for log in with XTR4L1F3
    Xtralife xtra = new Xtralife();

    void Start()
    {
        if (xtra.isSaveFileAlreadyCreated())
        {
            JToken token = JObject.Parse(File.ReadAllText(Directory.GetFiles(Application.persistentDataPath)[0]));
            string id = (string)token.SelectToken("gamer_id");
            string secret = (string)token.SelectToken("gamer_secret");
            xtra.ResumeSession(FindObjectOfType<CotcGameObject>(), id, secret);
        }
        else
        {
            GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = true;
        }
    }

    /////////////////////////////////
    ////////////* LOGIN *////////////
    /////////////////////////////////

    /// <summary>
    /// Will Login Anonymously.
    /// </summary>
    public void OnClickLoginAnonymous()
    {
        xtra.LoginAnonymous(FindObjectOfType<CotcGameObject>());
    }

    /// <summary>
    /// Will Login.
    /// </summary>
    public void OnClickLogin()
    {
        xtra.Login(FindObjectOfType<CotcGameObject>(), GameObject.Find("LoginInputField").GetComponent<InputField>().text, GameObject.Find("PasswordInputField").GetComponent<InputField>().text);
    }

    /// <summary>
    /// Will go to the xtralife website.
    /// </summary>
    public void OnClickCreateAccount()
    {
        System.Diagnostics.Process.Start("https://account.clanofthecloud.com/");
    }

    /// <summary>
    /// Will delete current account.
    /// </summary>
    public void OnClickDeleteAccount()
    {
        xtra.DeleteCurrentSaveFile();
    }

    /////////////////////////////////
    //////////* MAIN MENU *//////////
    /////////////////////////////////

    /// <summary>
    /// Will go to PLAY Menu.
    /// </summary>
    public void OnClickMainMenuPlay()
    {
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = true;
    }
    /// <summary>
    /// Will go to the OPTIONS Menu.
    /// </summary>
    public void OnClickMainMenuOptions()
    {
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuOptionsCanvas").GetComponent<Canvas>().enabled = true;
    }

    /// <summary>
    /// Log Out.
    /// </summary>
    public void OnClickMainMenuLogOut()
    {
        xtra.Logout(FindObjectOfType<CotcGameObject>());
    }

    /// <summary>
    /// Will quit the application.
    /// </summary>
    public void OnClickMainMenuQuit()
    {
        xtra.Logout(FindObjectOfType<CotcGameObject>());
        Application.Quit();
    }

    /// <summary>
    /// Will go the LANGUAGES Menu.
    /// </summary>
    public void OnClickMainMenuLanguages()
    {
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuLanguagesCanvas").GetComponent<Canvas>().enabled = true;
    }

    /// <summary>
    /// Will go the LEADERBOARD Menu.
    /// </summary>
    public void OnClickMainMenuLeaderboard()
    {
        xtra.BestHighScores();
        xtra.UserBestScores();
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuLeaderboard").GetComponent<Canvas>().enabled = true;
    }

    /////////////////////////////////
    /////////* LEADERBOARD */////////
    /////////////////////////////////

    /// <summary>
    /// Will go the MAIN MENU.
    /// </summary>
    public void OnClickLeaderBoardBack()
    {
        GameObject.Find("MenuLeaderboard").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    /////////////////////////////////
    //////////*   PLAY    *//////////
    /////////////////////////////////

    /// <summary>
    /// Will go the SINGLEPLAYER Menu.
    /// </summary>
    public void OnClickPlaySinglePlayer()
    {
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuSinglePlayerCanvas").GetComponent<Canvas>().enabled = true;
    }

    /// <summary>
    /// Will go the MULTIPLAYER Menu.
    /// </summary>
    public void OnClickPlayMultiPlayer()
    {
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMultiPlayerCanvas").GetComponent<Canvas>().enabled = true;
    }

    /// <summary>
    /// Will go the MAIN MENU.
    /// </summary>
    public void OnClickPlayBack()
    {
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    /////////////////////////////////
    ////////* SINGLE PLAYER *////////
    /////////////////////////////////

    /// <summary>
    /// When slider value change.
    /// Will change the Text value on the GUI and change Game Settings.
    /// </summary>
    public void OnValueChangedSinglePlayerSizeSlider()
    {
        int sizeValue = (int)GameObject.Find("SliderSize").GetComponent<Slider>().value;
        GameObject.Find("SizeValue").GetComponent<Text>().text = sizeValue.ToString();
        GameSettings.Size = sizeValue;
    }

    /// <summary>
    /// When editing value for the seed.
    /// Will change the Text value on the GUI and change Game Settings.
    /// </summary>
    public void OnEndEditSinglePlayerSeedInput()
    {
        int seedValue = 0;
        // check if value is not empty
        if (!GameObject.Find("SeedInputField").GetComponent<InputField>().text.Equals(""))
        {
            seedValue = int.Parse(GameObject.Find("SeedInputField").GetComponent<InputField>().text);
        }
        GameSettings.Seed = seedValue;
    }

    /// <summary>
    /// Will start loading the map and change scene.
    /// </summary>
    public void OnClickSinglePlayerStart()
    {
        GameObject.Find("MenuSinglePlayerCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuLoading").GetComponent<Canvas>().enabled = true;
        xtra.PostScore((int)xtra.userCurrentScore + 1);
        xtra.userCurrentScore++;
        StartCoroutine(LoadAsync("Terrain"));
    }

    /// <summary>
    /// Will go to the PLAY Menu.
    /// </summary>
    public void OnClickSinglePlayerBack()
    {
        GameObject.Find("MenuSinglePlayerCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = true;
    }

    /////////////////////////////////
    /////////* MULTIPLAYER */////////
    /////////////////////////////////

    /// <summary>
    /// Will go to the PLAY Menu.
    /// </summary>
    public void OnClickMultiPlayerBack()
    {
        GameObject.Find("MenuMultiPlayerCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = true;
    }

    /////////////////////////////////
    ///////////* LOADING *///////////
    /////////////////////////////////

    /// <summary>
    /// Load another scene async.
    /// </summary>
    IEnumerator LoadAsync(string sceneStr)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneStr);

        operation.allowSceneActivation = false;

        Text loadingNumberText = GameObject.Find("LoadingPercentage").GetComponent<Text>();
        Slider loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();

        while (loadingProgress < 1.0f)
        {
            loadingProgress = (float)(Mathf.Clamp01(operation.progress / 0.9f) * 1.0f);

            loadingNumberText.text = (int)(loadingProgress * 100f) + " %";
            loadingSlider.value = (float)(loadingProgress);

            yield return null;
        }

        loadingProgress = 0.0f;

        operation.allowSceneActivation = true;
    }

    /////////////////////////////////
    ///////////* OPTIONS *///////////
    /////////////////////////////////

    /// <summary>
    /// Will go to the MAIN MENU.
    /// </summary>
    public void OnClickOptionsBack()
    {
        GameObject.Find("MenuOptionsCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    /////////////////////////////////
    //////////* LANGUAGES *//////////
    /////////////////////////////////

    /// <summary>
    /// Will go to the MAIN MENU.
    /// </summary>
    public void OnClickLanguagesBack()
    {
        GameObject.Find("MenuLanguagesCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    /// <summary>
    /// Change the language to English.
    /// </summary>
    public void OnClickLanguagesEnglish()
    {
        CGT.LocalizationManager.instance.localizationConfiguration.currentLanguage = CGT.LocalizationConfiguration.Language.English;
    }

    /// <summary>
    /// Change the language to French.
    /// </summary>
    public void OnClickLanguagesFrench()
    {
        CGT.LocalizationManager.instance.localizationConfiguration.currentLanguage = CGT.LocalizationConfiguration.Language.French;
    }
}
