using Cysharp.Threading.Tasks;
using Preferences.Ini;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace Preferences
{
	/// <summary>
	/// Stores game-wide preferences.
	/// </summary>
	public class GamePreferences : PreferenceCategory
	{
		// Data

		/// <summary>
		/// Locale code used by Unity Localization, for example <c>en</c> or <c>ru</c>.
		/// </summary>
		public string LanguageCode { get; set; }

		// Metadata

		protected override string SectionName => "Game";

		// Lifecycle

		/// <inheritdoc />
		public override void New()
		{
			LanguageCode = GetDefaultLanguageCode();
		}

		/// <inheritdoc />
		public override async UniTask Apply()
		{
			if (!LocalizationSettings.HasSettings) {
				return;
			}

			Locale locale = ResolveLocale(LanguageCode)
			                ?? LocalizationSettings.SelectedLocale
			                ?? ResolveLocale("en")
			                ?? LocalizationSettings.AvailableLocales?.Locales?[0];

			if (locale == null) {
				return;
			}

			LanguageCode = locale.Identifier.Code;

			if (LocalizationSettings.SelectedLocale != locale) {
				LocalizationSettings.SelectedLocale = locale;
			}
		}

		// Serialization

		protected override void Read(IniSectionReader reader)
		{
			LanguageCode = reader.GetString(nameof(LanguageCode), LanguageCode);
		}

		protected override void Write(IniSectionWriter writer)
		{
			writer.Set(nameof(LanguageCode), LanguageCode);
		}

		// Helpers

		private static string GetDefaultLanguageCode()
		{
			if (LocalizationSettings.HasSettings && LocalizationSettings.SelectedLocale != null) {
				return LocalizationSettings.SelectedLocale.Identifier.Code;
			}

			return Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en";
		}

		private static Locale ResolveLocale(string code)
		{
			if (string.IsNullOrWhiteSpace(code) || !LocalizationSettings.HasSettings || LocalizationSettings.AvailableLocales == null) {
				return null;
			}

			foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales) {
				if (locale == null) {
					continue;
				}

				if (string.Equals(locale.Identifier.Code, code, System.StringComparison.OrdinalIgnoreCase)) {
					return locale;
				}
			}

			return null;
		}
	}
}