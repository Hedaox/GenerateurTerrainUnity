using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CGT
{
	public static class LocalizationProAPI
	{
		public static void SetCurrentLanguage(LocalizationConfiguration.Language language)
		{
			if(LocalizationManager.instance.localizationConfiguration.IsSet(language))
				LocalizationManager.instance.localizationConfiguration.currentLanguage=language;			
		}

		public static void SetDefaultLanguage(LocalizationConfiguration.Language language)
		{
			if (LocalizationManager.instance.localizationConfiguration.IsSet(language))
				LocalizationManager.instance.localizationConfiguration.defaultLanguage = language;
		}

		public static LocalizationConfiguration.Language GetCurrentLanguage(LocalizationConfiguration.Language language)
		{
			return LocalizationManager.instance.localizationConfiguration.currentLanguage;
		}

		public static LocalizationConfiguration.Language GetDefaultLanguage(LocalizationConfiguration.Language language)
		{
			return LocalizationManager.instance.localizationConfiguration.defaultLanguage;
		}
	}
}