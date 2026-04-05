using Infrastructure.Services;
using Preferences;
using UnityEngine;
using VContainer;


namespace Settings.Views
{
	/// <summary>
	/// Base class for a settings UI section.
	/// </summary>
	public abstract class SettingsSection : MonoBehaviour
	{
		// Dependencies

		[Inject] protected readonly PreferencesService PreferencesService;

		// State

		public PreferencesCategory UntypedPreferences { get; protected set; }

		// Lifecycle

		/// <summary>
		/// Resolves preferences dependencies for the section.
		/// </summary>
		public abstract void InitPreferences();

		/// <summary>
		/// Builds static UI state before values are loaded.
		/// </summary>
		public virtual void Build()
		{
		}

		/// <summary>
		/// Loads current preferences values into the UI without creating user-driven side effects.
		/// </summary>
		public abstract void Load();

		/// <summary>
		/// Binds UI events after the view has been synchronized with preferences.
		/// </summary>
		public virtual void Bind()
		{
		}
	}

	/// <summary>
	/// Typed base class for a settings UI section.
	/// </summary>
	public abstract class SettingsSection<T> : SettingsSection where T : PreferencesCategory
	{
		// State

		protected T Preferences { get; private set; }

		// Lifecycle

		/// <inheritdoc />
		public override void InitPreferences()
		{
			Preferences        = PreferencesService.Get<T>();
			UntypedPreferences = Preferences;
		}
	}
}