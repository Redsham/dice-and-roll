using Cysharp.Threading.Tasks;
using Preferences;
using System;
using UnityEngine;


namespace Infrastructure.Services
{
	/// <summary>
	/// Coordinates loading, applying and saving all preferences categories.
	/// </summary>
	public class PreferencesService
	{
		// State

		private readonly PreferenceCategory[] m_Categories = {
			new GamePreferences(), new GraphicsPreferences(),
			new AudioPreferences(), new ControlsPreferences()
		};

		// Access

		/// <summary>
		/// Returns a preference category by its concrete type.
		/// </summary>
		public T Get<T>() where T : PreferenceCategory
		{
			foreach (PreferenceCategory category in m_Categories) {
				if (category is T t) {
					return t;
				}
			}
			
			throw new($"Preference category of type {typeof(T)} not found");
		}

		// Lifecycle

		/// <summary>
		/// Creates a new preferences file from defaults and applies it immediately.
		/// </summary>
		public async UniTask New()
		{
			ResetAllToDefaults();
			await ApplyAll();
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences initialized");
			
			await Save();
		}

		/// <summary>
		/// Saves all categories to disk.
		/// </summary>
		public async UniTask Save()
		{
			await ForEachCategory(category => category.Save());
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences saved");
		}

		/// <summary>
		/// Loads all categories from disk, or creates defaults when the file does not exist.
		/// </summary>
		public async UniTask Load()
		{
			if (!Preferences.Ini.IniPreferencesStorage.Exists()) {
				await New();
				return;
			}

			await ForEachCategory(category => category.Load());
			await ApplyAll();
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences loaded");
		}

		// Helpers

		private void ResetAllToDefaults()
		{
			foreach (PreferenceCategory category in m_Categories) {
				category.New();
			}
		}

		private UniTask ApplyAll()
		{
			return ForEachCategory(category => category.Apply());
		}

		private async UniTask ForEachCategory(Func<PreferenceCategory, UniTask> action)
		{
			foreach (PreferenceCategory category in m_Categories) {
				await action(category);
			}
		}
	}
}
