using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace CGT
{
    [CustomEditor(typeof(LocalizationManager))]

    class LocalizationManagerEditor : Editor
    {
        static int flags = 0;
        int selected = 0;
        static List<string> components = new List<string>();

        private SerializedProperty localizationConfiguration, currentLanguage, ignoreGlobalLanguage;

        void OnEnable()
        {
            localizationConfiguration = serializedObject.FindProperty("localizationConfiguration");
            currentLanguage = serializedObject.FindProperty("currentLanguage");
            ignoreGlobalLanguage = serializedObject.FindProperty("ignoreGlobalLanguage");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            style.fontSize = 11;

            
            if (LocalizationManager.instance.localizationConfiguration == null)
                EditorGUILayout.HelpBox("LOCALIZATION CONFIGURATION\nYou need to set a LocalizationConfiguration scriptable object here. If you didn't create one already please, right-click on any folder of your project  and select\n\nCreate->Localization->LocalizationConfigration\n\nthen set the create object here.", MessageType.Error);
            else
                EditorGUILayout.TextArea("<b>LOCALIZATION CONFIGURATION</b>\nScriptable object to use in this scene.", style);

            EditorGUILayout.PropertyField(localizationConfiguration);
            localizationConfiguration.serializedObject.ApplyModifiedProperties();

            if (LocalizationManager.instance.localizationConfiguration == null)
                return;
            ThinLine();
            EditorGUILayout.TextArea("<b>CURRENT LANGUAGE</b>\nCurrent languaje for the scene. It is synced with the <b>Global Configuration</b> unless you override it by setting the 'Ignore Global Language' flag.", style);

            //Create the list 'usedLanguages' with all active languages
            string[] allLanguages = Enum.GetNames(typeof(LocalizationConfiguration.Language));
            List<string> usedLanguages = new List<string>();
            for (int i = 0, j=0; i < allLanguages.Length; i++)
                if ((LocalizationManager.instance.localizationConfiguration.maskValue & (1 << i)) != 0)
                {
                    usedLanguages.Add(allLanguages[i]);
                    if (currentLanguage.intValue == i)
                        selected = j;
                    j++;
                }

            selected = EditorGUILayout.Popup(selected, usedLanguages.ToArray());

            for (int i = 0, j=0; i < allLanguages.Length; i++)
                if ((LocalizationManager.instance.localizationConfiguration.maskValue & (1 << i)) != 0)
                {
                    if (j == selected)
                        currentLanguage.intValue = i;
                    j++;
                }
            currentLanguage.serializedObject.ApplyModifiedProperties();
            ThinLine();
            EditorGUILayout.TextArea("<b>IGNORE GLOBAL LANGUAGE</b>\nIf set, you can choose a different language for this scene than the Global Language.", style);
            ignoreGlobalLanguage.boolValue = EditorGUILayout.ToggleLeft("Ignore Global Language", ignoreGlobalLanguage.boolValue);
            ignoreGlobalLanguage.serializedObject.ApplyModifiedProperties();
        }

        void ThinLine(int i_height = 1)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = 4;
            GUILayout.Label("", style);
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            GUILayout.Label("", style);
        }
    }
}