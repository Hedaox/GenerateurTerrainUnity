using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace CGT
{
    [CustomEditor(typeof(LocalizableObject))]

    class LocalizableObjectEditor : Editor
    {
        static int flags = 0;
        int selected = 0;
        static List<string> components= new List<string>();

        private SerializedProperty property;

        void OnEnable()
        {
            property = serializedObject.FindProperty("localizableComponents");
        }

        public override void OnInspectorGUI()
        {           
            serializedObject.Update();
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            style.fontSize = 11;

            if (LocalizationManager.instance.localizationConfiguration == null)
            {
                EditorGUILayout.HelpBox("ERROR\nYou need to set the 'Localization Configuration' property on CGTManagers/LocalizationManager", MessageType.Error);
                return;
            }

            EditorGUILayout.TextArea("<b>COMPONENTS TO LOCALIZE</b>\nCheck the components to localize in this object or select the 'Autodetect' option. Below you will see a tab with all the availables languages where you can set the different localizations.", style);
            EditorGUILayout.PropertyField(property);

        }
    }
}