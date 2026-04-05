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

		private readonly PreferencesCategory[] m_Categories = {
			new GamePreferenceses(), new GraphicsPreferenceses(),
			new AudioPreferenceses(), new ControlsPreferenceses()
		};

		// Events

		public event Action<PreferencesCategory> CategoryChanged;

		// Access
		
		/// <summary>
		/// Indicates whether preferences have been loaded and applied at least once, and are thus ready to be used.
		/// </summary>
		public bool IsReady { get; private set; }

		/// <summary>
		/// Returns a preference category by its concrete type.
		/// </summary>
		public T Get<T>() where T : PreferencesCategory
		{
			foreach (PreferencesCategory category in m_Categories) {
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
			await Apply();
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences initialized");
			
			await Save();
			
			IsReady = true;
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
			await Apply();

			IsReady = true;
			
			Debug.Log($"[{nameof(PreferencesService)}] Preferences loaded");
		}
		
		/// <summary>
		/// Applies all categories to Unity runtime systems.
		/// </summary>
		public async UniTask Apply()
		{
			foreach (PreferencesCategory category in m_Categories) {
				await category.Apply();
				NotifyCategoryChanged(category);
			}
		}

		/// <summary>
		/// Notifies listeners that a category values were changed and may need to be re-applied externally.
		/// </summary>
		public void NotifyCategoryChanged(PreferencesCategory category)
		{
			CategoryChanged?.Invoke(category);
		}

		// Helpers

		private void ResetAllToDefaults()
		{
			foreach (PreferencesCategory category in m_Categories) {
				category.New();
			}
		}

		private async UniTask ForEachCategory(Func<PreferencesCategory, UniTask> action)
		{
			foreach (PreferencesCategory category in m_Categories) {
				await action(category);
			}
		}
	}
}
