using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // Loading progress when starting a game
    private static float loadingProgress = 0.0f;

    // MAIN MENU

    public void OnClickMainMenuPlay()
    {
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = true;
    }    
    public void OnClickMainMenuOptions()
    {
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuOptionsCanvas").GetComponent<Canvas>().enabled = true;
    }
    
    public void OnClickMainMenuQuit()
    {
        Application.Quit();
    }

    public void OnClickMainMenuLanguages()
    {
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuLanguagesCanvas").GetComponent<Canvas>().enabled = true;
    }

    // PLAY

    public void OnClickPlaySinglePlayer()
    {
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuSinglePlayerCanvas").GetComponent<Canvas>().enabled = true;
    }

    public void OnClickPlayMultiPlayer()
    {
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMultiPlayerCanvas").GetComponent<Canvas>().enabled = true;
    }

    public void OnClickPlayBack()
    {
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    // SINGLEPLAYER

    public void OnValueChangedSinglePlayerSizeSlider()
    {
        int sizeValue = (int)GameObject.Find("SliderSize").GetComponent<Slider>().value;
        GameObject.Find("SizeValue").GetComponent<Text>().text = sizeValue.ToString();
        GameSettings.Size = sizeValue;
    }

    public void OnEndEditSinglePlayerSeedInput()
    {
        int seedValue = 0;
        if (!GameObject.Find("SeedInputField").GetComponent<InputField>().text.Equals(""))
        {
            seedValue = int.Parse(GameObject.Find("SeedInputField").GetComponent<InputField>().text);
        }
        GameSettings.Seed = seedValue;
    }

    public void OnClickSinglePlayerStart()
    {
        GameObject.Find("MenuSinglePlayerCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuLoading").GetComponent<Canvas>().enabled = true;
        StartCoroutine(LoadAsync("Terrain"));
    }

    public void OnClickSinglePlayerBack()
    {
        GameObject.Find("MenuSinglePlayerCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = true;
    }

    // MULTIPLAYER

    public void OnClickMultiPlayerBack()
    {
        GameObject.Find("MenuMultiPlayerCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuPlayCanvas").GetComponent<Canvas>().enabled = true;
    }

    // LOADING

    // Load another scene async
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

    // OPTIONS

    public void OnClickOptionsBack()
    {
        GameObject.Find("MenuOptionsCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    // LANGUAGES

    public void OnClickLanguagesBack()
    {
        GameObject.Find("MenuLanguagesCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
    }

    public void OnClickLanguagesEnglish()
    {
        CGT.LocalizationManager.instance.localizationConfiguration.currentLanguage = CGT.LocalizationConfiguration.Language.English;
    }

    public void OnClickLanguagesFrench()
    {
        CGT.LocalizationManager.instance.localizationConfiguration.currentLanguage = CGT.LocalizationConfiguration.Language.French;
    }
}
