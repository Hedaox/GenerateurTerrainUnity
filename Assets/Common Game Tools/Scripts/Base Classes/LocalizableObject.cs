using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CGT
{

    public enum LocalizedType
    {
        stringValue,
        intValue,
        floatValue,
        boolValue,
        SpriteValue,
        AudioClipValue,
        MaterialValue
    }

    [Serializable]
    public class LocalizedValue
    {
        public string key;
        public string valueString;
        public int valueInt;
        public float valueFloat;
        public bool valueBool;
        public Sprite valueSprite;
        public AudioClip valueAudioClip;
        public Material valueMaterial;
        public LocalizedType type;
        public LocalizationConfiguration.Language language;
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(UniqueId))]
    public class LocalizableObject : MonoBehaviour
    {
        //Mask with components in the game object to be localized
        [ListMask] public int localizableComponents = 1;

        //name of the component properties to be localized
        //I.e. UnityEngine.UI.Text.text or UnityEngine.AudioSource.AudioClip
        public string[] localizableList = { };

        //Language values
        //The key is constructed as follows:
        //UID.Variable_name.Language
        public LocalizedValue[] localizedValues= { };

        private int currentLanguage = -1;   //For internal usaage

        //Registers itself as a Localizable Object
        void Start()
        {
            SetLocalizationValue();
        }

        //When uniqueId is updated we need to perform some actions
        public void UpdateUID( string oldUID)
        {
            List<LocalizedValue> oldValues = new List<LocalizedValue>(localizedValues);
            if (oldValues != null && oldUID.Length>0) { 
                for (int i = 0; i < oldValues.Count; i++)
                        oldValues[i].key = oldValues[i].key.Replace(oldUID, GetComponent<UniqueId>().uniqueId);
                    localizedValues = oldValues.ToArray();
            }
        }

        private void Update()
        {
			if(currentLanguage!=(int)LocalizationManager.instance.currentLanguage)
            {
                //Debug.Log(gameObject.name+" - From :" + ((LocalizationConfiguration.Language)currentLanguage).ToString() + " To: " + LocalizationManager.instance.currentLanguage.ToString());
                currentLanguage = (int)LocalizationManager.instance.currentLanguage;
                SetLocalizationValue();
            }
#if UNITY_EDITOR
			SetDefaultValue();
#endif
		}

		//Set all values for localization
		//If no valid value found then fallbacks to default language
		void SetLocalizationValue()
        {
            if (LocalizationManager.instance.localizationConfiguration == null)
                return;

            Component[] allComponents = GetComponents<Component>();
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
                        string toFind = GetComponent<UniqueId>().uniqueId + "."+allComponents[i].GetType() + "." + fields[j].Name;
                        LocalizedValue lv = FindValueForKey(toFind + "." + LocalizationManager.instance.currentLanguage.ToString());
                        LocalizedValue defaultLv = FindValueForKey(toFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString());

                        if (fields[j].FieldType == typeof(string) || fields[j].FieldType == typeof(String))
                        {
                            if(lv!=null && lv.valueString.Length>0)
                                fields[j].SetValue(allComponents[i], lv.valueString);
                            else if(defaultLv!=null)
                                fields[j].SetValue(allComponents[i], defaultLv.valueString);
                        }
                        if (fields[j].FieldType == typeof(Sprite))
                        {
                            if (lv != null && lv.valueSprite!=null)
                                fields[j].SetValue(allComponents[i], lv.valueSprite);
                            else if (defaultLv != null)
                                fields[j].SetValue(allComponents[i], defaultLv.valueSprite);
                        }
                        if (fields[j].FieldType == typeof(AudioClip))
                        {
                            if (lv != null && lv.valueAudioClip != null)
                                fields[j].SetValue(allComponents[i], lv.valueAudioClip);
                            else if (defaultLv != null)
                                fields[j].SetValue(allComponents[i], defaultLv.valueAudioClip);
                        }
                        if (lv != null && fields[j].FieldType == typeof(float))
                            fields[j].SetValue(allComponents[i], lv.valueFloat);
                        if (lv != null && fields[j].FieldType == typeof(int))
                            fields[j].SetValue(allComponents[i], lv.valueInt);
                        if (lv != null && fields[j].FieldType == typeof(bool))
                            fields[j].SetValue(allComponents[i], lv.valueBool);
                    }

                }
                else if (index > -1)
                {
                    string toFind = GetComponent<UniqueId>().uniqueId+".";
                    if (allComponents[i].GetType() == typeof(Text))
                        toFind += allComponents[i].GetType().ToString() + ".text";
					if (allComponents[i].GetType() == typeof(TextMeshProUGUI))
						toFind += allComponents[i].GetType().ToString() + ".text";
					else if (allComponents[i].GetType() == typeof(Image))
                        toFind += allComponents[i].GetType().ToString() + ".SourceImage";
                    else if (allComponents[i].GetType() == typeof(AudioSource))
                        toFind += allComponents[i].GetType().ToString() + ".AudioClip";


                    LocalizedValue lv = FindValueForKey(toFind + "." + LocalizationManager.instance.currentLanguage.ToString());
                    LocalizedValue defaultLv = FindValueForKey(toFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString());

                    if (allComponents[i].GetType() == typeof(Text))
                    {
                        if (lv != null && lv.valueString.Length > 0)
                            allComponents[i].gameObject.GetComponent<Text>().text= lv.valueString;
                        else if (defaultLv != null)
                            allComponents[i].gameObject.GetComponent<Text>().text = defaultLv.valueString;
                    }
					if (allComponents[i].GetType() == typeof(TextMeshProUGUI))
					{
						if (lv != null && lv.valueString.Length > 0)
							allComponents[i].gameObject.GetComponent<TextMeshProUGUI>().text = lv.valueString;
						else if (defaultLv != null)
							allComponents[i].gameObject.GetComponent<Text>().text = defaultLv.valueString;
					}
					if (allComponents[i].GetType() == typeof(Image))
                    {
                        if (lv != null && lv.valueSprite!=null)
                            allComponents[i].gameObject.GetComponent<Image>().sprite = lv.valueSprite;
                        else if (defaultLv != null)
                            allComponents[i].gameObject.GetComponent<Image>().sprite = defaultLv.valueSprite;
                    }
                    if (allComponents[i].GetType() == typeof(AudioSource))
                    {
                        if (lv != null && lv.valueAudioClip != null)
                            allComponents[i].gameObject.GetComponent<AudioSource>().clip = lv.valueAudioClip;
                        else if (defaultLv != null)
                            allComponents[i].gameObject.GetComponent<AudioSource>().clip = defaultLv.valueAudioClip;
                    }

                }
            }
            string mtoFind = GetComponent<UniqueId>().uniqueId + "." + "MainMaterial";
            LocalizedValue mlv = FindValueForKey(mtoFind + "." + LocalizationManager.instance.currentLanguage.ToString());
            LocalizedValue defaultMlv = FindValueForKey(mtoFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString());
            if(mlv != null && mlv.valueMaterial!=null)
                GetComponent<Renderer>().sharedMaterial = mlv.valueMaterial;
            else if(defaultMlv!=null && defaultMlv.valueMaterial != null)
                GetComponent<Renderer>().sharedMaterial = defaultMlv.valueMaterial;

        }

		//If the SET DEFAULT flag is checked the set the current object value
		//to the default language if it's empty
		void SetDefaultValue()
		{
			if (LocalizationManager.instance.localizationConfiguration == null)
				return;

			List<LocalizedValue> newLvs = new List<LocalizedValue>(localizedValues);
			bool newValuesAdded = false;

			Component[] allComponents = GetComponents<Component>();
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
						string toFind = GetComponent<UniqueId>().uniqueId + "." + allComponents[i].GetType() + "." + fields[j].Name;
						LocalizedValue defaultLv = FindValueForKey(toFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString());

						if(defaultLv==null)
						{
							defaultLv = new LocalizedValue();							
							defaultLv.key = toFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString();
							defaultLv.language = LocalizationManager.instance.localizationConfiguration.defaultLanguage;
							newLvs.Add(defaultLv);
							newValuesAdded = true;
						}

						if ((fields[j].FieldType == typeof(string) || fields[j].FieldType == typeof(String)) && 
							(defaultLv.valueString==null || defaultLv.valueString.Length == 0)) {
							defaultLv.valueString = (string)fields[j].GetValue(allComponents[i]);
							defaultLv.type = LocalizedType.stringValue;
						}
						if (fields[j].FieldType == typeof(Sprite) && defaultLv.valueSprite==null)
						{
							defaultLv.valueSprite = (Sprite)fields[j].GetValue(allComponents[i]);
							defaultLv.type = LocalizedType.SpriteValue;
						}
						if (fields[j].FieldType == typeof(AudioClip) && defaultLv.valueAudioClip == null)
						{
							defaultLv.valueAudioClip = (AudioClip)fields[j].GetValue(allComponents[i]);
							defaultLv.type = LocalizedType.AudioClipValue;
						}
					}

				}
				else if (index > -1)
				{
					string toFind = GetComponent<UniqueId>().uniqueId + ".";
					if (allComponents[i].GetType() == typeof(Text))
						toFind += allComponents[i].GetType().ToString() + ".text";
					if (allComponents[i].GetType() == typeof(TextMeshProUGUI))
						toFind += allComponents[i].GetType().ToString() + ".text";
					else if (allComponents[i].GetType() == typeof(Image))
						toFind += allComponents[i].GetType().ToString() + ".SourceImage";
					else if (allComponents[i].GetType() == typeof(AudioSource))
						toFind += allComponents[i].GetType().ToString() + ".AudioClip";

					LocalizedValue defaultLv = FindValueForKey(toFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString());

					if (defaultLv == null)
					{
						defaultLv = new LocalizedValue();
						defaultLv.key = toFind + "." + LocalizationManager.instance.localizationConfiguration.defaultLanguage.ToString();
						defaultLv.language = LocalizationManager.instance.localizationConfiguration.defaultLanguage;
						newLvs.Add(defaultLv);
						newValuesAdded = true;
					}

					if (allComponents[i].GetType() == typeof(Text) && (defaultLv.valueString == null || defaultLv.valueString.Length == 0))
					{
						defaultLv.valueString = allComponents[i].gameObject.GetComponent<Text>().text;
						defaultLv.type = LocalizedType.stringValue;
					}
					if (allComponents[i].GetType() == typeof(TextMeshProUGUI) && (defaultLv.valueString == null || defaultLv.valueString.Length == 0))
					{
						defaultLv.valueString = allComponents[i].gameObject.GetComponent<TextMeshProUGUI>().text;
						defaultLv.type = LocalizedType.stringValue;
					}
					if (allComponents[i].GetType() == typeof(Image) && defaultLv.valueSprite == null)
					{
						defaultLv.valueSprite = allComponents[i].gameObject.GetComponent<Image>().sprite;
						defaultLv.type = LocalizedType.SpriteValue;
					}
					if (allComponents[i].GetType() == typeof(AudioSource) && defaultLv.valueAudioClip == null)
					{
						defaultLv.valueAudioClip = allComponents[i].gameObject.GetComponent<AudioSource>().clip;
						defaultLv.type = LocalizedType.AudioClipValue;
					}
				}
			}
			
			if(newValuesAdded)
				localizedValues = newLvs.ToArray();
		}

		public LocalizedValue FindValueForKey(string key)
        {
            for (int i = 0; i < localizedValues.Length; i++)
                if (localizedValues[i].key.Equals(key))
                    return localizedValues[i];
            return null;
        }
    }
}