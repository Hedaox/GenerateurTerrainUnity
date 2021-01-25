using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;

namespace CGT //Common Game Tools
{
	using TMPro;
#if UNITY_EDITOR
	using UnityEditor;
    [CustomPropertyDrawer(typeof(ListMaskAttribute))]
    public class ListMaskPropertyDrawer : PropertyDrawer
    {
        int maskValue, currentLanguage;
        Material defaultMaterial;

        static List<string> components = new List<string>(); //Potental list of components to be localized

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * components.Count;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string autodetected="";
           
            LocalizableObject locObj = (LocalizableObject)property.serializedObject.targetObject;

            Component[] allComponents = locObj.GetComponents<Component>();
            Renderer renderer = locObj.GetComponent<Renderer>();
            
            components.Clear();
            components.Add("Autodetect");

            //Retrieves all components and variables thar potentially can be localized
            for (int i = 0; i < allComponents.Length; i++)
            {
                //Check if the component type is in the list of localizable types
                int index = Array.IndexOf(LocalizationManager.instance.localizationConfiguration.typesToLocalize, allComponents[i].GetType());

                //If is a custom component, then look for defined atributes
                if (index >= LocalizationManager.instance.localizationConfiguration.numStandardTypes)
                {
                    const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                    FieldInfo[] fields = allComponents[i].GetType().GetFields(flags);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        if (Array.IndexOf(LocalizationManager.instance.localizationConfiguration.atributesToLocalize, fields[j].FieldType) > -1)
                        {
                            components.Add(allComponents[i].GetType() + "." + fields[j].Name);
                        }
                    }

                }
                else if (index > -1)
                {
                    if (allComponents[i].GetType() == typeof(Text))
                    {
                        autodetected = allComponents[i].GetType().ToString() + ".text";
                        components.Add(allComponents[i].GetType().ToString() + ".text");
                    }
					else if (allComponents[i].GetType() == typeof(TextMeshProUGUI))
					{
						if (autodetected.Length == 0)
							autodetected = allComponents[i].GetType().ToString() + ".text";
						components.Add(allComponents[i].GetType().ToString() + ".text");
					}
					else if (allComponents[i].GetType() == typeof(Image))
                    {
                        if (autodetected.Length == 0)
                            autodetected = allComponents[i].GetType().ToString()+ ".SourceImage";
                        components.Add(allComponents[i].GetType().ToString() + ".SourceImage");
                    }
                    else if (allComponents[i].GetType() == typeof(AudioSource))
                    {
                        if (autodetected.Length == 0)
                            autodetected = allComponents[i].GetType().ToString() + ".AudioClip";
                        components.Add(allComponents[i].GetType().ToString() + ".AudioClip");
                    }					
				}
            }
            if (renderer != null)
            {
                defaultMaterial = renderer.sharedMaterial;
                if (autodetected.Length == 0)
                    autodetected = "MainMaterial";
                components.Add("MainMaterial");
            }
            //At this point 'components' contains a list localizable components in the gameobject

            maskValue = property.intValue;
            
            EditorGUI.BeginProperty(position, label, property);            
            
            //Draw the check list of components
            for (int i = 0; i < components.Count; i++)
            {
                if (EditorGUI.ToggleLeft(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * i, position.width, EditorGUIUtility.singleLineHeight), components[i], IsSet(i)))
                {
                    ToggleIndex(i, true);
                }
                else
                {
                    ToggleIndex(i, false);
                }
            }
            

            GuiLine();

            //Create the list 'usedLanguages' with all active languages
            string[] allLanguages = Enum.GetNames(typeof(LocalizationConfiguration.Language));
            List<string> usedLanguages = new List<string>();
            for (int i = 0; i < allLanguages.Length; i++)
                if ((LocalizationManager.instance.localizationConfiguration.maskValue & (1 << i)) != 0)
                    usedLanguages.Add(allLanguages[i]);

            //A trick to clear the field when changing language tab
            int oldLang = currentLanguage;
            currentLanguage = GUILayout.Toolbar(currentLanguage, usedLanguages.ToArray());
            if(oldLang!=currentLanguage)
                EditorGUI.FocusTextInControl(null);
            
            //Draws a field placeholder for each different type (string, float, int, bool, audioclip, sprite)
            DrawEditorForProperties(property, (LocalizationConfiguration.Language)System.Enum.Parse(typeof(LocalizationConfiguration.Language), usedLanguages[currentLanguage]));

            EditorGUI.EndProperty();

            //Set the 'localizableList' values
            SerializedProperty localizableList = property.serializedObject.FindProperty("localizableList");
            if (maskValue!= property.intValue || localizableList.arraySize==0)
            {
                property.intValue = maskValue;

                localizableList.arraySize = 0;
                if (IsSet(0))
                {
                    localizableList.arraySize = localizableList.arraySize + 1;
                    localizableList.GetArrayElementAtIndex(localizableList.arraySize - 1).stringValue = autodetected;
                }
                else
                {
                    for (int i = 1; i < components.Count; i++)
                        if (IsSet(i))
                        {
                            localizableList.arraySize = localizableList.arraySize + 1;
                            localizableList.GetArrayElementAtIndex(localizableList.arraySize - 1).stringValue = components[i];
                        }
                }                
            }

            /*
            if (GUILayout.Button("Print Options"))
            {
                Debug.Log(maskValue);
                Debug.Log(autodetected);
                
                for (int i = 0; i < localizableList.arraySize; i++)
                {
                    Debug.Log("Value: "+localizableList.GetArrayElementAtIndex(i).stringValue);
                }
                for (int i = 0; i < components.Count; i++)
                {
                    Debug.Log("CValue: " + components[i]);
                }
            }
            */

            property.serializedObject.ApplyModifiedProperties();
        }

        //Draws and editor for all the selectred propierties
        void DrawEditorForProperties(SerializedProperty property, LocalizationConfiguration.Language language)
        {
            LocalizableObject locObj = (LocalizableObject)property.serializedObject.targetObject;
            Component[] allComponents = locObj.GetComponents<Component>();
            for (int i = 0; i < allComponents.Length; i++)
            {
                int index = Array.IndexOf(LocalizationManager.instance.localizationConfiguration.typesToLocalize, allComponents[i].GetType());

                //If is a custom component, then look for defined atributes
                if (index >= LocalizationManager.instance.localizationConfiguration.numStandardTypes)
                {
                    const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                    FieldInfo[] fields = allComponents[i].GetType().GetFields(flags);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        if (Array.IndexOf(LocalizationManager.instance.localizationConfiguration.atributesToLocalize, fields[j].FieldType) > -1)
                        {
                            string potential=allComponents[i].GetType() + "." + fields[j].Name;
                            if (ArrayUtility.Contains(locObj.localizableList, potential))
                            {                                
                                if (fields[j].FieldType == typeof(string))
                                    DrawEditorForType(potential, property, language, LocalizedType.stringValue);
                                else if (fields[j].FieldType == typeof(AudioClip))
                                    DrawEditorForType(potential, property, language, LocalizedType.AudioClipValue);
                                else if (fields[j].FieldType == typeof(Sprite))
                                    DrawEditorForType(potential, property, language, LocalizedType.SpriteValue);                                
                                else if (fields[j].FieldType == typeof(float))
                                    DrawEditorForType(potential, property, language, LocalizedType.floatValue);
                                else if (fields[j].FieldType == typeof(int))
                                    DrawEditorForType(potential, property, language, LocalizedType.intValue);
                                else if (fields[j].FieldType == typeof(bool))
                                    DrawEditorForType(potential, property, language, LocalizedType.boolValue);
                            }
                        }
                    }

                }
                else if (index > -1)
                {                    
                    if (allComponents[i].GetType() == typeof(Text))
                    {
                        string potential = allComponents[i].GetType().ToString() + ".text";
                        if (ArrayUtility.Contains(locObj.localizableList, potential))
                            DrawEditorForType(potential, property, language, LocalizedType.stringValue);
                    }
					else if (allComponents[i].GetType() == typeof(TextMeshProUGUI))
					{
						string potential = allComponents[i].GetType().ToString() + ".text";
						if (ArrayUtility.Contains(locObj.localizableList, potential))
							DrawEditorForType(potential, property, language, LocalizedType.stringValue);
					}
					else if (allComponents[i].GetType() == typeof(Image))
                    {
                        string potential = allComponents[i].GetType().ToString() + ".SourceImage";
                        if (ArrayUtility.Contains(locObj.localizableList, potential))
                            DrawEditorForType(potential, property, language, LocalizedType.SpriteValue);
                    }
                    else if (allComponents[i].GetType() == typeof(AudioSource))
                    {
                        string potential = allComponents[i].GetType().ToString() + ".AudioClip";
                        if (ArrayUtility.Contains(locObj.localizableList, potential))
                            DrawEditorForType(potential, property, language, LocalizedType.AudioClipValue);
                    }                    
                }                
            }
            if (true) {
                string potential = "MainMaterial";
                if (ArrayUtility.Contains(locObj.localizableList, potential))
                    DrawEditorForType(potential, property, language, LocalizedType.MaterialValue);
            }
        }

        //Draws a specific editor for each type
        //Currently supported types are:
        // string, float, int, boo, Sprite, AudioClip
        void DrawEditorForType(string objType, SerializedProperty property, LocalizationConfiguration.Language language, LocalizedType type)
        {
            LocalizableObject locObj = (LocalizableObject)property.serializedObject.targetObject;

            string key = locObj.GetComponent<UniqueId>().uniqueId + "." + objType + "." + language.ToString();
            string label = StringPathUtils.GetLastMembers(objType, '.', 2);
            string valueString = "";
            Sprite valueSprite = null;
            AudioClip valueAudioclip = null;
            Material valueMaterial = null;
            float valueFloat = 0.0f;
            int valueInt = 0;
            bool valueBool = false;
            int index = -1;

            SerializedProperty localizedValues = property.serializedObject.FindProperty("localizedValues");
            SerializedProperty elementArray = null;
            for (int i = 0; i < localizedValues.arraySize; i++)
            {
                elementArray = localizedValues.GetArrayElementAtIndex(i);
                if (elementArray.FindPropertyRelative("key").stringValue.Equals(key))
                {
                    switch(type)
                    {
                        case LocalizedType.stringValue:
                            valueString = elementArray.FindPropertyRelative("valueString").stringValue;
                            break;
                        case LocalizedType.AudioClipValue:
                            valueAudioclip = (AudioClip)elementArray.FindPropertyRelative("valueAudioClip").objectReferenceValue;
                            break;
                        case LocalizedType.SpriteValue:
                            valueSprite = (Sprite)elementArray.FindPropertyRelative("valueSprite").objectReferenceValue;
                            break;
                        case LocalizedType.MaterialValue:
                            valueMaterial = (Material)elementArray.FindPropertyRelative("valueMaterial").objectReferenceValue;
                            if (valueMaterial == null)
                                valueMaterial = defaultMaterial;
                            break;
                        case LocalizedType.floatValue:
                            valueFloat = elementArray.FindPropertyRelative("valueFloat").floatValue;
                            break;
                        case LocalizedType.intValue:
                            valueInt = elementArray.FindPropertyRelative("valueInt").intValue;
                            break;
                        case LocalizedType.boolValue:
                            valueBool = elementArray.FindPropertyRelative("valueBool").boolValue;
                            break;
                    }
                    index = i;
                    break;
                }
            }

            //GUILayout.Label(label);

            switch (type)
            {
                case LocalizedType.stringValue:
					GUILayout.Label(label);
					valueString = EditorGUILayout.TextArea(valueString, GUILayout.Height(50));
                    break;
                case LocalizedType.AudioClipValue:
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label);
                    valueAudioclip = (AudioClip)EditorGUILayout.ObjectField(valueAudioclip, typeof(AudioClip), false);
                    GUILayout.EndHorizontal();
                    break;
                case LocalizedType.SpriteValue:
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label);
                    valueSprite = (Sprite)EditorGUILayout.ObjectField(valueSprite, typeof(Sprite), false);
                    GUILayout.EndHorizontal();
                    break;
                case LocalizedType.MaterialValue:
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Main Material");
                    valueMaterial = (Material)EditorGUILayout.ObjectField(valueMaterial, typeof(Material), false);
                    GUILayout.EndHorizontal();
                    break;
                case LocalizedType.floatValue:
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label);
                    valueFloat = EditorGUILayout.FloatField(valueFloat);
                    GUILayout.EndHorizontal();
                    break;
                case LocalizedType.intValue:
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label);
                    valueInt = EditorGUILayout.IntField(valueInt);
                    GUILayout.EndHorizontal();
                    break;
                case LocalizedType.boolValue:
                    valueBool = EditorGUILayout.ToggleLeft(label, valueBool); break;
            }

            if (index >= 0)
            {
                elementArray = localizedValues.GetArrayElementAtIndex(index);                
                elementArray.FindPropertyRelative("type").intValue = (int)type;
                elementArray.FindPropertyRelative("language").intValue = (int)language;
            }
            else
            {
                localizedValues.arraySize = localizedValues.arraySize + 1;
                elementArray = localizedValues.GetArrayElementAtIndex(localizedValues.arraySize - 1);
                elementArray.FindPropertyRelative("key").stringValue = key;
                elementArray.FindPropertyRelative("type").intValue = (int)type;
                elementArray.FindPropertyRelative("language").intValue = (int)language;
            }

            switch (type)
            {
                case LocalizedType.stringValue:
                    elementArray.FindPropertyRelative("valueString").stringValue = valueString;
                    break;
                case LocalizedType.AudioClipValue:
                    elementArray.FindPropertyRelative("valueAudioClip").objectReferenceValue = valueAudioclip;
                    break;
                case LocalizedType.SpriteValue:
                    elementArray.FindPropertyRelative("valueSprite").objectReferenceValue = valueSprite;
                    break;
                case LocalizedType.MaterialValue:
                    elementArray.FindPropertyRelative("valueMaterial").objectReferenceValue = valueMaterial;
                    break;
                case LocalizedType.floatValue:
                    elementArray.FindPropertyRelative("valueFloat").floatValue = valueFloat;
                    break;
                case LocalizedType.intValue:
                    elementArray.FindPropertyRelative("valueInt").intValue = valueInt;
                    break;
                case LocalizedType.boolValue:
                    elementArray.FindPropertyRelative("valueBool").boolValue = valueBool;
                    break;
            }
        }
        
        void ToggleIndex(int index, bool isOn)
        {
            if (isOn)
            {
                if (index == 0)
                    maskValue = 1;
                else
                {
                    maskValue &= ~1;
                    maskValue |= (1 << index);
                }
            }
            else
                maskValue &= ~(1 << index);
        }

        bool IsSet(int index)
        {
            if ((maskValue & (1 << index)) != 0)
                return true;
            return false;
        }

        void GuiLine(int i_height = 1)
        {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            
            //Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            //rect.height = i_height;
            //EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            
        }


    }
#endif
}