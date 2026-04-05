using Infrastructure.Services;
using Preferences;
using UnityEngine;
using VContainer;


namespace Settings.Views
{
	public abstract class SettingsSection : MonoBehaviour
	{
		[Inject] protected readonly PreferencesService  PreferencesService;
		public                      PreferencesCategory UntypedPreferences { get; protected set; }

		public abstract void InitPreferences();
		public abstract void Init();
		public abstract void Load();
	}

	public abstract class SettingsSection<T> : SettingsSection where T : PreferencesCategory
	{
		protected T Preferences { get; private set; }

		public override void InitPreferences()
		{
			Preferences        = PreferencesService.Get<T>();
			UntypedPreferences = Preferences;
		}
	}
}