using System.Collections.Generic;
using Preferences;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace Settings.Views.Sections
{
	public class GameSection : SettingsSection<GamePreferenceses>
	{
		[SerializeField] private TMP_Dropdown m_LanguageDropdown;

		
		public override void Init()
		{
			m_LanguageDropdown.ClearOptions();

			List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
			foreach (Locale locale in locales) {
				m_LanguageDropdown.options.Add(new(locale.name));
			}

			m_LanguageDropdown.RefreshShownValue();

			m_LanguageDropdown.onValueChanged.AddListener(index => {
				LocalizationSettings.SelectedLocale                    = locales[index];
				Preferences.LanguageCode = locales[index].Identifier.Code;
			});
		}
		public override void Load()
		{
			List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
			m_LanguageDropdown.SetValueWithoutNotify(locales.IndexOf(LocalizationSettings.SelectedLocale));
		}
	}
}