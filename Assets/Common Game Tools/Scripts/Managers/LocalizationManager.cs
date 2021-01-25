using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CGT
{
    [ExecuteInEditMode]
    public class LocalizationManager : MonoBehaviour
    {
        //Scriptable object with all configuration
        public LocalizationConfiguration localizationConfiguration;
        public bool ignoreGlobalLanguage = false;   

        private static LocalizationManager _instance = null;
        public static LocalizationManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (LocalizationManager)FindObjectOfType(typeof(LocalizationManager));
                    if (_instance == null)
                    {
                        GameObject managers = GameObject.Find("/CGTManagers");
                        if (managers == null)
                            managers = new GameObject("CGTManagers");
                        _instance = (new GameObject("LocalizationManager")).AddComponent<LocalizationManager>();
                        _instance.transform.parent = managers.transform;
                    }
                }
                return _instance;
            }
        }

        //Current language of the game
        //System automatically detects changes in this value and apply language
        public LocalizationConfiguration.Language currentLanguage;

        private void Update()
        {
            if (localizationConfiguration!=null && currentLanguage != localizationConfiguration.currentLanguage && !ignoreGlobalLanguage)
            {                
                //Debug.Log("From :" + currentLanguage.ToString() + " To: " + localizationConfiguration.currentLanguage.ToString());
                currentLanguage = localizationConfiguration.currentLanguage;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {   //Forcing a second Update to call Update on ALL objects [ExecuteInEditMode]
                    UnityEditor.EditorApplication.delayCall += UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
                    //For some extrange undocumented reason UnityEditor.EditorApplication.delayCall is 
                    //cleaned after each Update call, so we need to += QueuePlayerLoopUpdate method every time
                }
#endif
            }
        }

        public void Initialize()
        {
            //For future use
        }        

        public void LoadLanguageText()
        {

        }
    }
}