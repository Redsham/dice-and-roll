using Cysharp.Threading.Tasks;
using Preferences;
using UnityEngine;


namespace Infrastructure.Services
{
	public class PreferencesService
	{
		private readonly PreferenceCategory[] Categories = {
			new GamePreferences(), new GraphicsPreferences(),
			new AudioPreferences(), new ControlsPreferences()
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
			foreach (PreferenceCategory category in Categories) {
				category.New();
			}
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences initialized");
			
			Save();
		}
		public async UniTask Save()
		{
			foreach (PreferenceCategory category in Categories) {
				await category.Save();
			}
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences saved");
		}
		public async UniTask Load()
		{
			foreach (PreferenceCategory category in Categories) {
				await category.Load();
			}
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences loaded");
		}
	}
}