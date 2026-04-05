using Preferences;
using UnityEngine;


namespace Infrastructure.Services
{
	public class PreferencesService
	{
		private readonly PreferenceCategory[] Categories = {
			new GamePreferences(), new GraphicsPreferences(),
			new AudioPrefrences(), new ControlsPreferences()
		};

		public T Get<T>() where T : PreferenceCategory
		{
			foreach (PreferenceCategory category in Categories) {
				if (category is T t) {
					return t;
				}
			}
			
			throw new($"Preference category of type {typeof(T)} not found");
		}

		public void New()
		{
			foreach (var category in Categories) {
				category.New();
			}
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences initialized");
			
			Save();
		}
		public void Save()
		{
			foreach (var category in Categories) {
				category.Save();
			}
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences saved");
		}
		public void Load()
		{
			foreach (var category in Categories) {
				category.Load();
			}
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences loaded");
		}
	}
}