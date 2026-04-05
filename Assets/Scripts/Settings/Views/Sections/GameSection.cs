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

		private List<Locale> m_Locales;


		public override void Build()
		{
			m_LanguageDropdown.ClearOptions();

			m_Locales = LocalizationSettings.AvailableLocales.Locales;
			foreach (Locale locale in m_Locales) {
				m_LanguageDropdown.options.Add(new(locale.name));
			}

			m_LanguageDropdown.RefreshShownValue();
		}

		public override void Bind()
		{
			m_LanguageDropdown.onValueChanged.AddListener(index => {
				LocalizationSettings.SelectedLocale = m_Locales[index];
				Preferences.LanguageCode            = m_Locales[index].Identifier.Code;
			});
		}

		public override void Load()
		{
			m_LanguageDropdown.SetValueWithoutNotify(m_Locales.IndexOf(LocalizationSettings.SelectedLocale));
		}
	}
}