using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEditor.SceneManagement;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using System.Text;
using TMPro;

namespace CGT
{
    [CustomEditor(typeof(LocalizationConfiguration))]

    class LocalizationConfigurationEditor : Editor
    {
        static string path_to_files = "Common Game Tools/Resources/CGT_";
        int _maskValue, _choiceIndex, _choiceCurrent, _typeMaskValue, _clearMaskValue;
        static string[] languages;

        private SerializedProperty defaultLanguage, currentLanguage, maskValue, scenes, saveWithComments, currentAsDefault;
        private bool listVisibility = true, withComments, curAsDefault;

        int _currentLanguage;
        void OnEnable()
        {
            defaultLanguage = serializedObject.FindProperty("defaultLanguage");
            currentLanguage = serializedObject.FindProperty("currentLanguage");
            maskValue = serializedObject.FindProperty("maskValue");
            scenes = serializedObject.FindProperty("scenes");
            saveWithComments = serializedObject.FindProperty("saveWithComments");
			currentAsDefault = serializedObject.FindProperty("currentAsDefault");
		}

        public override void OnInspectorGUI()
        {
            languages = Enum.GetNames(typeof(LocalizationConfiguration.Language));
            _maskValue = maskValue.intValue;
            List<string> availableLanguages = new List<string>();
            for (int i = 0, j = 0; i < languages.Length; i++)
                if (IsSet(i))
                {
                    if (defaultLanguage.enumValueIndex == i)
                        _choiceIndex = j;
                    if (currentLanguage.enumValueIndex == i)
                        _choiceCurrent = j;
                    availableLanguages.Add(languages[i]);
                    j++;
                }
            

            serializedObject.Update();

            LocalizationConfiguration localizationConfiguration = (LocalizationConfiguration)target;

            GUIStyle styleCaption = new GUIStyle(EditorStyles.largeLabel);
            styleCaption.wordWrap = true;
            styleCaption.richText = true;
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            style.fontSize = 11;

            //GuiLine();
            EditorGUILayout.TextArea("<b>GAME LANGUAGES</b>\nCheck all the languages that you wish to use in your game", style);
            //Draw the list
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            for (int i = 0; i < languages.Length; i++)
            {
                if(i==languages.Length/2 && languages.Length>3)
                {
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                }
                if (EditorGUILayout.ToggleLeft(languages[i], IsSet(i)))
                {
                    ToggleIndex(i, true);
                }
                else
                {
                    ToggleIndex(i, false);
                }                
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            ThinLine(1);

            EditorGUILayout.TextArea("<b>CURRENT LANGUAGE</b>\nCurrent language for all the scenes yin the game", style);
            _choiceCurrent = EditorGUILayout.Popup("Current language", _choiceCurrent, availableLanguages.ToArray());

            ThinLine(1);

            EditorGUILayout.TextArea("<b>DEFAULT LANGUAGE</b>\nIs the language to use when no localization exists for an object", style);
            _choiceIndex = EditorGUILayout.Popup("Default language", _choiceIndex, availableLanguages.ToArray());

			ThinLine(1);

			EditorGUILayout.TextArea("<b>SET DEFAULT VALUE</b>\nIf the default languaje value of the object is empty then set the current object value to it.", style);
			currentAsDefault.boolValue = EditorGUILayout.ToggleLeft("Set object value to default language", currentAsDefault.boolValue);
			curAsDefault = currentAsDefault.boolValue;

			GuiLine();            

            GUILayout.Label("<b>BATCH PROCESSING</b>\nOptional section to manage the whole game project. We encourage you to read carefully the manual and use with caution.", styleCaption);
            //EditorGUILayout.TextArea("<b>BATCH PROCESSING</b>\nOptional section to manage the whole game project. We encourage you to read carefully the manual and use with caution.\n\nNotice that this actions only works with standard serializable types (string, int, float and bool) it doesn't works with localized Sprites or Audioclips. You must set manually in editor the values of this objects.", style);
            GUILayout.Label("");
            EditorGUILayout.TextArea("<b>SCENES TO LOCALIZE</b>\nAdd the scene files to manage.", style);
            EditorGUILayout.PropertyField(scenes, new GUIContent("Scenes to localize"), true);                        
            
            ThinLine(1);
            EditorGUILayout.TextArea("<b>LANGUAGE FILES</b>\nExport/Import localization text.\n\n<b>EXPORT</b> => Create a file with the localized text currently existing in the scenes.\n\n<b>IMPORT</b> => Replace the localized text currently existing in the scenes with the contents of the file.\n\nNotice that only <b>string</b> type will be exported/imported. This tool doesn't works with any other localized type (Sprites, Audioclips, Materials, int, bool or float).", style);
            ThinSpace(2);
            saveWithComments.boolValue = EditorGUILayout.ToggleLeft("Add comments to the file", saveWithComments.boolValue);
            withComments = saveWithComments.boolValue;
			
			ThinSpace(2);
            ThinLine(1);
            EditorGUILayout.TextArea("Usual production pipeline is to create the game using only the default language, then EXPORT the file, send this file to translators which will return back the file localized and then IMPORT each of the files in the specific localization.", style);
            
            for (int i = 0; i < languages.Length; i++)
                if (IsSet(i))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(languages[i], GUILayout.Width(200));

                    if (GUILayout.Button("EXPORT"))
                    {
                        var path = EditorUtility.SaveFilePanel(
                            "Export localization as CSV",
                            "",
                            languages[i].ToString() + ".csv",
                            "csv");

                        if (path.Length != 0)
                            SimpleEditorCoroutine.Start(ExportFileForLanguage((LocalizationConfiguration.Language)i, localizationConfiguration.scenes, path));
                        //EditorCoroutineUtility.StartCoroutine(ExportFileForLanguage((LocalizationConfiguration.Language)i, localizationConfiguration.scenes, path), this);
                    }
                    if (GUILayout.Button("IMPORT"))
                    {
                        var path = EditorUtility.OpenFilePanel(
                            "Import "+languages[i]+" localization from CSV",
                            "",
                            "csv");

                        if (path.Length != 0)
                            SimpleEditorCoroutine.Start(ImportFileForLanguage((LocalizationConfiguration.Language)i, localizationConfiguration.scenes, path));
                        //EditorCoroutineUtility.StartCoroutine(ImportFileForLanguage((LocalizationConfiguration.Language)i, localizationConfiguration.scenes, path), this);
                    }
                    
                    GUILayout.EndHorizontal();
                }

            ThinLine(1);
            EditorGUILayout.TextArea("If you preffer you can export ALL LANGUAGES to a single file. This pipeline is usual for small projects where only one person makes all translations.", style);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("EXPORT ALL"))
            {
                if (EditorUtility.DisplayDialog("EXPORT ALL",
                        "This action export all languages in one single file",
                        "Ok", "Cancel"))
                {
                    var path = EditorUtility.SaveFilePanel(
                            "Export all localization as CSV",
                            "",
                            "AllLanguages.csv",
                            "csv");

                    if (path.Length != 0)
                        SimpleEditorCoroutine.Start(ExportFileForAllLanguages(localizationConfiguration.scenes, path));
                    //EditorCoroutineUtility.StartCoroutine(ExportFileForAllLanguages(localizationConfiguration.scenes, path), this);
                }
            }
            if (GUILayout.Button("IMPORT ALL"))
            {
                if (EditorUtility.DisplayDialog("IMPORT ALL",
                        "This action import all languages from one single file",
                        "Ok", "Cancel"))
                {
                    var path = EditorUtility.OpenFilePanel(
                            "Import all localization as CSV",
                            ".",
                            "csv");

                    if (path.Length != 0)
                        SimpleEditorCoroutine.Start(ImportFileForAllLanguages(localizationConfiguration.scenes, path));
                    //EditorCoroutineUtility.StartCoroutine(ImportFileForAllLanguages(localizationConfiguration.scenes, path), this);
                }
            }
            GUILayout.EndHorizontal();


            ThinLine(1);
            EditorGUILayout.TextArea("<b>AUTO LOZALIZE TYPES</b>\nAutomatically adds 'LocalizableObject' script to all objects of selected type found in the scenes to localize that doesn't has already the script and set the current value of the object to the current languaje slot.", style);
            //Draw the types list
            string typesOn = "";
            string typesOff = "";

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            for (int i = 0; i < localizationConfiguration.typesToLocalize.Length; i++)
            {
                if (EditorGUILayout.Toggle(localizationConfiguration.typesToLocalize[i].ToString(), TypeIsSet(i)))
                {
                    TypeToggleIndex(i, true);
                }
                else
                {
                    TypeToggleIndex(i, false);
                }
                if (TypeIsSet(i))
                    typesOn += "\n" + localizationConfiguration.typesToLocalize[i].ToString();
                else
                    typesOff += "\n" + localizationConfiguration.typesToLocalize[i].ToString();

            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUIStyle btnBign = new GUIStyle(GUI.skin.button);
            btnBign.fontSize = 14;
            btnBign.fontStyle = FontStyle.Bold;
            btnBign.padding = new RectOffset(4, 4, 4, 4);

            if (GUILayout.Button(" Add Localization ", btnBign))
            {
                if (typesOn.Length == 0)
                {
                    EditorUtility.DisplayDialog("WARNING", "There is no objects selected!\nNothing to do...", "Ok");
                }
                else if (EditorUtility.DisplayDialog("Remove Localization?",
                        "This action will add localization of the objects CHECKED in the list:\n" + typesOn,
                        "Ok", "Cancel"))
                {
                    SimpleEditorCoroutine.Start(AddRemoveLocalization(localizationConfiguration));
                    //EditorCoroutineUtility.StartCoroutine(AddRemoveLocalization(localizationConfiguration), this);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ThinLine(1);


            EditorGUILayout.TextArea("<b>REMOVE LOCALIZATION</b>\nAutomatically removes 'LocalizableObject' script from all objects of selected type found in the scenes to localize. Use with caution.", style);
            //Draw the types list
            string ctypesOn = "";
            string ctypesOff = "";
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            for (int i = 0; i < localizationConfiguration.typesToLocalize.Length; i++)
            {
                if (EditorGUILayout.Toggle(localizationConfiguration.typesToLocalize[i].ToString(), ClearIsSet(i)))
                {
                    ClearToggleIndex(i, true);
                }
                else
                {
                    ClearToggleIndex(i, false);
                }
                if (ClearIsSet(i))
                    ctypesOn += "\n" + localizationConfiguration.typesToLocalize[i].ToString();
                else
                    ctypesOff += "\n" + localizationConfiguration.typesToLocalize[i].ToString();

            }

            
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            var oldColor2 = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUIStyle btnBig = new GUIStyle(GUI.skin.button);
            btnBig.fontSize = 14;
            btnBig.normal.textColor = Color.white;
            btnBig.fontStyle = FontStyle.Bold;
            btnBig.padding = new RectOffset(4, 4, 4, 4);

            if (GUILayout.Button(" Remove Localization ", btnBig))
            {
                if (ctypesOn.Length == 0)
                {
                    EditorUtility.DisplayDialog("WARNING", "All objects are selected!\nNothing to do...", "Ok");
                }
                else if (EditorUtility.DisplayDialog("Remove Localization?",
                        "This action will remove localization of the objects CHECKED in the list:" + ctypesOn,
                        "Ok", "Cancel"))
                {
                    SimpleEditorCoroutine.Start(AddRemoveLocalization(localizationConfiguration, true));
                    //EditorCoroutineUtility.StartCoroutine(AddRemoveLocalization(localizationConfiguration, true), this);
                }

            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.backgroundColor = oldColor2;


            for (int i = 0, j = 0; i < languages.Length; i++)
                if (IsSet(i))
                {
                    if(_choiceCurrent==j)
                        currentLanguage.enumValueIndex = i;
                    if (_choiceIndex == j)
                        defaultLanguage.enumValueIndex = i;
                    j++;
                }

            _currentLanguage = currentLanguage.intValue;
            currentLanguage.serializedObject.ApplyModifiedProperties();
            maskValue.intValue = _maskValue;
            maskValue.serializedObject.ApplyModifiedProperties();
            saveWithComments.serializedObject.ApplyModifiedProperties();
			currentAsDefault.serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        void ToggleIndex(int index, bool isOn)
        {
            if (isOn)
                _maskValue |= (1 << index);
            else
                _maskValue &= ~(1 << index);
            if (_maskValue == 0)
                _maskValue |= (1 << index);
        }

        void TypeToggleIndex(int index, bool isOn)
        {
            if (isOn)
                _typeMaskValue |= (1 << index);
            else
                _typeMaskValue &= ~(1 << index);
        }

        void ClearToggleIndex(int index, bool isOn)
        {
            if (isOn)
                _clearMaskValue |= (1 << index);
            else
                _clearMaskValue &= ~(1 << index);
        }

        bool IsSet(int index)
        {
			LocalizationProAPI.SetCurrentLanguage(LocalizationConfiguration.Language.Spanish);
			if ((_maskValue & (1 << index)) != 0)
                return true;
            return false;
        }

        bool TypeIsSet(int index)
        {
            if ((_typeMaskValue & (1 << index)) != 0)
                return true;
            return false;
        }

        bool ClearIsSet(int index)
        {
            if ((_clearMaskValue & (1 << index)) != 0)
                return true;
            return false;
        }

        void GuiLine()
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = 8;
            GUILayout.Label("", style);
            int i_height = 4;
            //EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            GUILayout.Label("", style);
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
        void ThinSpace(int i_height = 1)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = i_height;
            GUILayout.Label("", style);            
        }
        
        IEnumerator ExportFileForLanguage(LocalizationConfiguration.Language lan, List<SceneAsset> scenes, string path)
        {
            int lineNumber = 1;
            String key="";
            String value="";
            String objectName = "";
            string fileContent = "";
			string objContent = "";
            int total = 0;
			fileContent += "# Encoding "+Encoding.Default+ "\n";
			if (withComments)
                fileContent +="###############################\n#\n# LOCALIZATION FILE\n# Language: " + lan.ToString()+ "\n# Creation Date: " + System.DateTime.Now + "\n#\n###############################\n";
            string currentPath= EditorSceneManager.GetActiveScene().path;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            //For each scene in the list
            for (int i = 0; i < scenes.Count; i++)
            {
                if (withComments)
                    fileContent += "###############################\n#\n# SCENE NAME: " + scenes[i].name+"\n#\n###############################\n";
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[i]));
                 //For each localizable object
                foreach (LocalizableObject obj in Resources.FindObjectsOfTypeAll<LocalizableObject>())
                {
                    GameObject locObj = obj.gameObject;
                    Component[] allComponents = locObj.GetComponents<Component>();

                    //Retrieves all components and variables thar potentially can be localized
                    for (int j = 0; j < allComponents.Length; j++)
                    {
                        bool found = false;
                        //Check if the component type is in the list of localizable types
                        int index = Array.IndexOf(LocalizationManager.instance.localizationConfiguration.typesToLocalize, allComponents[j].GetType());

                        //If is a custom component, then look for defined atributes
                        if (index >= LocalizationManager.instance.localizationConfiguration.numStandardTypes)
                        {
                            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                            FieldInfo[] fields = allComponents[j].GetType().GetFields(flags);
                            for (int k = 0; k < fields.Length; k++)
                            {
                                if (fields[k].FieldType==typeof(string) || fields[k].FieldType == typeof(String))
                                {
									objContent = (string)fields[k].GetValue(allComponents[j]);
									key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + "." + fields[k].Name + "." + lan.ToString();
                                    value = "";
                                    objectName = locObj.name;
                                    LocalizedValue lv = obj.FindValueForKey(key);
                                    if (lv != null)
                                        value = lv.valueString;                                    
                                    found = true;
                                }
                            }

                        }
                        else if (index > -1)
                        {
                            if (allComponents[j].GetType() == typeof(Text) || allComponents[j].GetType() == typeof(TextMeshProUGUI))
                            {
								if(allComponents[j].GetType() == typeof(Text))
									objContent = allComponents[j].gameObject.GetComponent<Text>().text;
								else
									objContent = allComponents[j].gameObject.GetComponent<TextMeshProUGUI>().text;
								key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + ".text" + "." + lan.ToString();
                                value = "";
                                objectName = locObj.name;
                                LocalizedValue lv = obj.FindValueForKey(key);
                                if (lv != null)
                                    value = lv.valueString;
                                found = true;
                            }
                        }
                        if (found)
                        {
							//if (withComments)
							//	fileContent += "#;#;#;"+objContent+"\n";
							fileContent += lineNumber++ + ",\"" + key.Replace("\"", "\"\"") + "\",\"" + objectName.Replace("\"", "\"\"") + "\",\""  + value.Replace("\"", "\"\"") + "\",\"" + objContent.Replace("\"", "\"\"") + "\"\n";
                            total++;
                        }
                    }
                }
            }

            //Returns to starting scene
            if(currentPath.Length>0)
                EditorSceneManager.OpenScene(currentPath);

            bool error = false;
            try
            {
					File.WriteAllText(path, fileContent, Encoding.UTF8);
			}
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error Saving File", "File cannot be saved. Please see console log to get more information.", "Ok");
                Debug.LogException(e);
                error = true;
            }

            if (!error)
                EditorUtility.DisplayDialog("File Saved", total + " values exported correctly.", "Ok");

            yield return null;
        }

        IEnumerator ExportFileForAllLanguages(List<SceneAsset> scenes, string path)
        {
            int lineNumber = 1;
            String key = "";
            String value = "";
            String objectName = "";
            string fileContent = "";
			string objContent = "";
			int total = 0;
			fileContent += "# Encoding " + Encoding.Default + "\n";
			if (withComments)
                fileContent += "###############################\n#\n# LOCALIZATION FILE\n# All Languages\n# Creation Date: " + System.DateTime.Now + "\n#\n###############################\n";
            string currentPath = EditorSceneManager.GetActiveScene().path;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            //For each scene in the list
            for (int i = 0; i < scenes.Count; i++)
            {
                if (withComments)
                    fileContent += "###############################\n#\n# SCENE NAME: " + scenes[i].name + "\n#\n###############################\n";
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[i]));
                //For each localizable object
                foreach (LocalizableObject obj in Resources.FindObjectsOfTypeAll<LocalizableObject>())
                {
                    GameObject locObj = obj.gameObject;
                    Component[] allComponents = locObj.GetComponents<Component>();
                    
                    //Retrieves all components and variables thar potentially can be localized
                    for (int j = 0; j < allComponents.Length; j++)
                    {
                        bool objHeaderPrinted = false;
                        //Check if the component type is in the list of localizable types
                        int index = Array.IndexOf(LocalizationManager.instance.localizationConfiguration.typesToLocalize, allComponents[j].GetType());

                        //If is a custom component, then look for defined atributes
                        if (index >= LocalizationManager.instance.localizationConfiguration.numStandardTypes)
                        {
                            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                            FieldInfo[] fields = allComponents[j].GetType().GetFields(flags);
                            for (int k = 0; k < fields.Length; k++)
                            {
                                if (fields[k].FieldType == typeof(string) || fields[k].FieldType == typeof(String))
                                {                                    
                                    if(!objHeaderPrinted && withComments)
                                    {
                                        fileContent += "# "+locObj.name +"."+ allComponents[j].GetType() + "." + fields[k].Name+"\n";
                                        objHeaderPrinted = true;
                                    }
									objContent = (string)fields[k].GetValue(allComponents[j]);
									//if (withComments)
									//	fileContent += "#;#;#;" + objContent + "\n";
									for (int p = 0; p < languages.Length; p++)
                                        if (IsSet(p))
                                        {											
											key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + "." + fields[k].Name + "." + languages[p].ToString();
                                            value = "";
                                            objectName = locObj.name;
                                            LocalizedValue lv = obj.FindValueForKey(key);
                                            if (lv != null)
                                                value = lv.valueString;
                                            else if (p == _currentLanguage)
                                                value = (string)fields[k].GetValue(allComponents[j]);
                                            fileContent += lineNumber++ + ",\"" + key.Replace("\"", "\"\"") + "\",\"" + objectName.Replace("\"", "\"\"") + "\",\"" + value.Replace("\"", "\"\"") + "\",\"" + objContent.Replace("\"", "\"\"") + "\"\n";
                                            total++;
                                        }                                    
                                }
                            }

                        }
                        else if (index > -1)
                        {
							if (allComponents[j].GetType() == typeof(Text) || allComponents[j].GetType() == typeof(TextMeshProUGUI))
							{
								if (allComponents[j].GetType() == typeof(Text))
									objContent = allComponents[j].gameObject.GetComponent<Text>().text;
								else
									objContent = allComponents[j].gameObject.GetComponent<TextMeshProUGUI>().text;
								if (!objHeaderPrinted && withComments)
                                {
                                    fileContent += "#\n# " + locObj.name + "."+ allComponents[j].GetType() + "\n";
                                    objHeaderPrinted = true;
                                }
								
								//if (withComments)
								//	fileContent += "#;#;#;" + objContent + "\n";
								for (int p = 0; p < languages.Length; p++)
                                    if (IsSet(p))
                                    {										
										key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + ".text" + "." + languages[p].ToString();
                                        value = "";
                                        objectName = locObj.name;
                                        LocalizedValue lv = obj.FindValueForKey(key);
                                        if (lv != null)
                                            value = lv.valueString;
                                        else if (p == _currentLanguage)
                                            value = ((Text)allComponents[j]).text;
                                        fileContent += lineNumber++ + ",\"" + key.Replace("\"", "\"\"") + "\",\"" + objectName.Replace("\"", "\"\"") + "\",\"" + value.Replace("\"", "\"\"") + "\",\"" + objContent.Replace("\"", "\"\"") + "\"\n";
                                        total++;
                                    }                                
                            }
                        }
                    }
                }
            }

            //Returns to starting scene
            if (currentPath.Length > 0)
                EditorSceneManager.OpenScene(currentPath);

            bool error = false;
            try
            {
				File.WriteAllText(path, fileContent, Encoding.UTF8);
			}
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error Saving File", "File cannot be saved. Please see console log to get more information.", "Ok");
                Debug.LogException(e);
                error = true;
            }

            if(!error)
                EditorUtility.DisplayDialog("File Saved", total + " values exported correctly.", "Ok");
            
            yield return null;
        }
        
        IEnumerator ImportFileForLanguage(LocalizationConfiguration.Language lan, List<SceneAsset> scenes, string path)
        {
            bool error = false;
            String[] lines = { };
            try
            {
                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error OPening File", "File cannot be opened. Please see console log to get more information.", "Ok");
                Debug.LogException(e);
                error = true;
            }

            if (error)
                yield return null;

            Dictionary<string, string> localizedValues = new Dictionary<string, string>();

            //CSV format is :
            // first field => ordinal, only for informational purpouses
            // key => used to match with Unity object
            // Object name => name of the object, only for informational purpouses
            // Value => Localized text value
            for(int i=0; i<lines.Length; i++)
            {
                if (lines[i][0] == '#')
                    continue;
                String[] fields = GetAllFields(i, out int newLine, lines);
                i = newLine;
                if(fields.Length>3)
                    localizedValues.Add(fields[1], fields[3]);
            }
            
            String key = "";
            string currentPath = EditorSceneManager.GetActiveScene().path;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            int imported = 0;
            int created = 0;

            //For each scene in the list
            for (int i = 0; i < scenes.Count; i++)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[i]));
                //For each localizable object
                foreach (LocalizableObject obj in Resources.FindObjectsOfTypeAll<LocalizableObject>())
                {
                    GameObject locObj = obj.gameObject;
                    Component[] allComponents = locObj.GetComponents<Component>();
                    List<LocalizedValue> newLvs = new List<LocalizedValue>();
                    //Retrieves all components and variables thar potentially can be localized
                    for (int j = 0; j < allComponents.Length; j++)
                    {
                        //Check if the component type is in the list of localizable types
                        int index = Array.IndexOf(LocalizationManager.instance.localizationConfiguration.typesToLocalize, allComponents[j].GetType());

                        //If is a custom component, then look for defined atributes
                        if (index >= LocalizationManager.instance.localizationConfiguration.numStandardTypes)
                        {
                            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                            FieldInfo[] fields = allComponents[j].GetType().GetFields(flags);
                            for (int k = 0; k < fields.Length; k++)
                            {
                                if (fields[k].FieldType == typeof(string) || fields[k].FieldType == typeof(String))
                                {
                                    key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + "." + fields[k].Name + "." + lan.ToString();
                                    if (localizedValues.ContainsKey(key))
                                    {
                                        LocalizedValue lv = obj.FindValueForKey(key);
                                        if (lv != null)
                                        {
                                            lv.valueString = localizedValues[key];
                                            imported++;
                                        }
                                        else
                                        {
                                            LocalizedValue newLv = new LocalizedValue();
                                            newLv.valueString = localizedValues[key];
                                            newLv.key = key;
                                            newLv.type = LocalizedType.stringValue;
                                            newLv.language = lan;
                                            newLvs.Add(newLv);
                                            created++;
                                        }
                                    }                                    
                                }
                            }

                        }
                        else if (index > -1)
                        {
							if (allComponents[j].GetType() == typeof(Text) || allComponents[j].GetType() == typeof(TextMeshProUGUI))
							{
                                key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + ".text" + "." + lan.ToString();
                                if (localizedValues.ContainsKey(key))
                                {
                                    LocalizedValue lv = obj.FindValueForKey(key);
                                    if (lv != null)
                                    {
                                        imported++;
                                        lv.valueString = localizedValues[key];
                                    }
                                    else
                                    {
                                        LocalizedValue newLv = new LocalizedValue();
                                        newLv.valueString = localizedValues[key];
                                        newLv.key = key;
                                        newLv.type = LocalizedType.stringValue;
                                        newLv.language = lan;
                                        newLvs.Add(newLv);
                                        created++;
                                    }
                                }
                            }
                        }                        
                    }
                    List<LocalizedValue> oldLvs = new List<LocalizedValue>(obj.localizedValues);
                    oldLvs.AddRange(newLvs);
                    obj.localizedValues = oldLvs.ToArray();
                }
                //Save scene
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            //Returns to starting scene
            if (currentPath.Length > 0)
                EditorSceneManager.OpenScene(currentPath);

            if (!error)
                EditorUtility.DisplayDialog("File imported", imported+" values imported and "+created+" new values created.", "Ok");
            yield return null;
        }

        IEnumerator ImportFileForAllLanguages(List<SceneAsset> scenes, string path)
        {
            bool error = false;
            String[] lines = { };
            try
            {
                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error OPening File", "File cannot be opened. Please see console log to get more information.", "Ok");
                Debug.LogException(e);
                error = true;
            }

            if(error)                
                yield return null;

            Dictionary<string, string> localizedValues = new Dictionary<string, string>();

            //CSV format is :
            // first field => ordinal, only for informational purpouses
            // key => used to match with Unity object
            // Object name => name of the object, only for informational purpouses
            // Value => Localized text value
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i][0] == '#')
                    continue;
                String[] fields = GetAllFields(i, out int newLine, lines);
                i = newLine;
                if (fields.Length > 3)
                    localizedValues.Add(fields[1], fields[3]);
            }

            String key = "";
            string currentPath = EditorSceneManager.GetActiveScene().path;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            int imported = 0;
            int created = 0;
            //For each scene in the list
            for (int i = 0; i < scenes.Count; i++)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[i]));
                //For each localizable object
                foreach (LocalizableObject obj in Resources.FindObjectsOfTypeAll<LocalizableObject>())
                {
                    GameObject locObj = obj.gameObject;
                    Component[] allComponents = locObj.GetComponents<Component>();
                    List<LocalizedValue> newLvs = new List<LocalizedValue>();
                    //Retrieves all components and variables thar potentially can be localized
                    for (int j = 0; j < allComponents.Length; j++)
                    {
                        //Check if the component type is in the list of localizable types
                        int index = Array.IndexOf(LocalizationManager.instance.localizationConfiguration.typesToLocalize, allComponents[j].GetType());

                        //If is a custom component, then look for defined atributes
                        if (index >= LocalizationManager.instance.localizationConfiguration.numStandardTypes)
                        {
                            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                            FieldInfo[] fields = allComponents[j].GetType().GetFields(flags);
                            for (int k = 0; k < fields.Length; k++)
                            {
                                if (fields[k].FieldType == typeof(string) || fields[k].FieldType == typeof(String))
                                {
                                    for (int p = 0; p < languages.Length; p++)
                                        if (IsSet(p))
                                        {

                                            key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + "." + fields[k].Name + "." + languages[p];
                                            if (localizedValues.ContainsKey(key))
                                            {
                                                LocalizedValue lv = obj.FindValueForKey(key);
                                                if (lv != null)
                                                {
                                                    lv.valueString = localizedValues[key];
                                                    imported++;
                                                }
                                                else
                                                {
                                                    LocalizedValue newLv = new LocalizedValue();
                                                    newLv.valueString = localizedValues[key];
                                                    newLv.key = key;
                                                    newLv.type = LocalizedType.stringValue;
                                                    newLv.language = (LocalizationConfiguration.Language)p;
                                                    newLvs.Add(newLv);
                                                    created++;
                                                }
                                            }
                                        }
                                }
                            }

                        }
                        else if (index > -1)
                        {
							if (allComponents[j].GetType() == typeof(Text) || allComponents[j].GetType() == typeof(TextMeshProUGUI))
							{
                                for (int p = 0; p < languages.Length; p++)
                                    if (IsSet(p))
                                    {
                                        key = locObj.GetComponent<UniqueId>().uniqueId + "." + allComponents[j].GetType() + ".text" + "." + languages[p];
                                        if (localizedValues.ContainsKey(key))
                                        {
                                            LocalizedValue lv = obj.FindValueForKey(key);
                                            if (lv != null)
                                            {
                                                lv.valueString = localizedValues[key];
                                                imported++;
                                            }
                                            else
                                            {
                                                LocalizedValue newLv = new LocalizedValue();
                                                newLv.valueString = localizedValues[key];
                                                newLv.key = key;
                                                newLv.type = LocalizedType.stringValue;
                                                newLv.language = (LocalizationConfiguration.Language)p;
                                                newLvs.Add(newLv);
                                                created++;
                                            }
                                        }
                                    }
                            }
                        }
                    }
                    List<LocalizedValue> oldLvs = new List<LocalizedValue>(obj.localizedValues);
                    oldLvs.AddRange(newLvs);
                    obj.localizedValues = oldLvs.ToArray();
                }
                //Save scene
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            //Returns to starting scene
            if (currentPath.Length > 0)
                EditorSceneManager.OpenScene(currentPath);

            if (!error)
                EditorUtility.DisplayDialog("File imported", imported + " values imported and " + created + " new values created.", "Ok");
            yield return null;
        }

        IEnumerator AddRemoveLocalization(LocalizationConfiguration lc, bool remove=false)
        {
            int total = 0;
            string currentPath = EditorSceneManager.GetActiveScene().path;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            //For each scene in the list
            for (int i = 0; i < lc.scenes.Count; i++)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(lc.scenes[i]));

                if(LocalizationManager.instance.localizationConfiguration==null)
                    LocalizationManager.instance.localizationConfiguration = (LocalizationConfiguration)target;

                foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    for (int j = 0; j < lc.typesToLocalize.Length; j++)
                    {
                        if (!remove && TypeIsSet(j))
                        {
                            if (obj.GetComponent<LocalizableObject>() == null && obj.GetComponent(lc.typesToLocalize[j]) != null)
                            {
                                LocalizableObject lo = obj.AddComponent<LocalizableObject>();
                                total++;
                                /*
                                for (int k=0; k< Enum.GetValues(typeof(LocalizationConfiguration.Language)).Length; k++)
                                    if(k==_currentLanguage)
                                    {
                                        LocalizedValue lv = new LocalizedValue();
                                        lv.language = (LocalizationConfiguration.Language)k;
                                        lv.type = LocalizedType.stringValue;
                                        lv.key = lo.GetComponent<UniqueId>().uniqueId + lc.typesToLocalize[j].ToString()+ Enum.GetNames(typeof(LocalizationConfiguration.Language))[k];
                                        lo.localizedValues = new LocalizedValue[1];
                                        lo.localizedValues[0] = lv;
                                    }                                                                
                                */
                            }
                        }
                        else if(remove && ClearIsSet(j))
                        {
                            if (obj.GetComponent<LocalizableObject>() != null && obj.GetComponent(lc.typesToLocalize[j]) != null)
                            {
                                total++;
                                DestroyImmediate(obj.GetComponent<LocalizableObject>());
                            }
                        }
                    }
                }
                
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            //Returns to starting scene
            if (currentPath.Length > 0)
                EditorSceneManager.OpenScene(currentPath);

            if (!remove)
                EditorUtility.DisplayDialog("Scripts added", total + " 'Localizable Object' scripts added.", "Ok");
            else
                EditorUtility.DisplayDialog("Scripts removed", total + " 'Localizable Object' scripts removed.", "Ok");
            yield return null;
        }                

        TextAsset SaveFileInResources(string text, string path)
        {
            File.WriteAllText(path, text);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            TextAsset textAsset = Resources.Load(name) as TextAsset;
            Debug.Log("Saved File: " + name + "\n"+Application.dataPath + "/"+path_to_files + name + ".csv");
            return textAsset;            
        }

        String[] GetAllFields(int i, out int line, String[] lines)
        {
            List<String> fields = new List<string>();
            //Quit double quotes
            //String clean=lines[i].Replace("\"\"", "\"");
            String value;

            line = i;
            int deltaPos = 0, totalPos = 0;
            while (totalPos < lines[line].Length)
            {
                value = "";
                String subLine = lines[line].Substring(totalPos);
                deltaPos = FieldLength(subLine, out bool quoted);
                while (deltaPos < 0 && line < lines.Length)
                {
                    deltaPos = 0;
                    value += subLine + "\n";
                    line++;
                    subLine = lines[line].Substring(deltaPos);
                    deltaPos = FieldLength(subLine, out bool dummy, true);
                    totalPos = 0;
                }
                value += subLine.Substring(0, deltaPos);
                deltaPos++;
                totalPos += deltaPos;
                if (quoted) //eliminate quotation
                    value = value.Substring(1, value.Length - 2);
                value = value.Replace("\"\"", "\"");
                fields.Add(value);
            }

            return fields.ToArray();
        }

        int FieldLength(String value, out bool quoted, bool forceQuote = false)
        {
            quoted = false;

            if (value.Length == 0)
                return -1;
            if (value[0] != '"' && !forceQuote)
            {
                //If do not start with " looks for ; or end of line
                int index = value.IndexOf(",", 0);
                if (index == -1)
                    index = value.Length;
                return index;
            }
            else
            {
                quoted = true;
                //Starts with a "
                int startPos = 1;
                if (forceQuote)
                    startPos = 0;
                for (int i = startPos; i < value.Length; i++)
                {
                    if (value[i] == '"' && i < value.Length - 1 && value[i + 1] == '"')
                    {   //If we find a "" then jump over
                        i++;
                        continue;
                    }
                    //else if is a " and next value is end of line returns length
                    else if (value[i] == '"' && i == value.Length - 1)
                        return value.Length;
                    //else if is a " and next is ; returns position of ;
                    else if (value[i] == '"' && i < value.Length - 1 && value[i + 1] == ',')
                        return i + 1;
                }
                //multi-line field
                return -1;
            }
        }
    }
}