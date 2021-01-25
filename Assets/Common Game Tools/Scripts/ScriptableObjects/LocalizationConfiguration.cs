using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CGT
{
    [CreateAssetMenu(fileName = "LocalizationConfiguration", menuName = "Localization/Localization Configuration", order = 1)]
    public class LocalizationConfiguration : ScriptableObject
    {

        //Fell free to add your own language at the end of the enum
        //Never change the order of the enum elements!!
        public enum Language
        {
            English,    // 0
            Chinese,    // 1
            Spanish,    // 2
            Japanese,   // 3
            French,     // 4
            German,     // 5
            Korean,     // 6
            Russian     // 7
        }

        //Mask values with selected languages
        //set by the custom editor
        public int maskValue = 1;

        //List of types that can be localized
        //Please: ADD YOU OWN CLASSES IF NEEDED!!
        //For example, if you have youw own object with a string field
        //that must be localized, add here this class name
        //NOTE: MainMaterial is added by default if exists
        public System.Type[] typesToLocalize = new[]
        {
            typeof(Text),
            typeof(Image),
            typeof(AudioSource),
			typeof(TextMeshProUGUI)
        };
		//IMPORTANT: For the tutorials there are two objects added 'MyOwnObject' and 'OwnMouseOver' 
		//you can delete it for your project


		//Potential atributes to localize in your custom classes
		//Usually 'string' is enough. Try not to abuse of this list
		//LocalizationManager has default editors for:
		// -string
		// -int
		// -float
		// -bool
		// -Sprite
		// -AudioClip
		//Any other field is no supported by custom editor
		//If you need an specific type to be supported,ask us, please.
		public System.Type[] atributesToLocalize = new[]
        {
            typeof(string),
			typeof(AudioClip)
        };

        //The number of standard classes to localize
        //In ths case is 4: UI.Text, UI.Image, AudioSource and TextMeshProUGUI
        //Above this number the script assumes are your own classes
        public int numStandardTypes = 4;

        //Current language of the game
        public Language currentLanguage;

        //If a resource is not found then it will be presented in this language
        public Language defaultLanguage = Language.English;

		//By default, if the valur of the default language of an object is enmpty, 
		//then set the object value.
		public bool currentAsDefault = true;

		//Save file with some comments (scene, object name, etc...)
		public bool saveWithComments = false;

		//Returns true if the language is set
		public bool IsSet(Language language)
		{
			if ((maskValue & (1 << (int)language)) != 0)
				return true;
			return false;
		}

#if UNITY_EDITOR
		public List<UnityEditor.SceneAsset> scenes = new List<UnityEditor.SceneAsset>();
#endif
    }
}