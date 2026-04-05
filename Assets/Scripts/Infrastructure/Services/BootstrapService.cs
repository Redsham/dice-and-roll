using Cysharp.Threading.Tasks;
using Infrastructure.Services.Scenes;
using R3;
using UI.Effects;


namespace Infrastructure.Services
{
	/// <summary>
	/// Initializes bootstrap-time services before the main flow continues.
	/// </summary>
	public class BootstrapService
	{
		// State

		/// <summary>
		/// Indicates that bootstrap initialization has finished.
		/// </summary>
		public bool                     Initialized { get; private set; } = false;
		/// <summary>
		/// Human-readable bootstrap progress message.
		/// </summary>
		public ReactiveProperty<string> Message     { get; }              = new();
		
		// Dependencies

		private readonly SceneService       m_SceneService;
		private readonly UIFade             m_Fade;
		private readonly PreferencesService m_Preferences;

		// Construction

		/// <summary>
		/// Creates a bootstrap service instance.
		/// </summary>
		public BootstrapService(SceneService sceneService, UIFade fade, PreferencesService preferences)
		{
			m_SceneService = sceneService;
			m_Fade = fade;
			m_Preferences = preferences;
		}
		
		// Lifecycle

		/// <summary>
		/// Initializes bootstrap-time systems.
		/// </summary>
		public async UniTask Initialize()
		{
			await LoadSettings();
			Initialized = true;
		}

		/// <summary>
		/// Loads a game scene.
		/// </summary>
		public async UniTask LoadScene(string sceneName)
		{
			Message.Value = "Loading scene...";
			await m_SceneService.LoadSceneAsync(sceneName);
		}

		/// <summary>
		/// Loads the main menu scene with a fade transition.
		/// </summary>
		public async UniTask LoadMenu()
		{
			Message.Value = "Loading menu...";
			await m_Fade.ActionAsync(async () => {
				await UniTask.WaitForSeconds(0.25f);
				await m_SceneService.LoadSceneAsync("Menu");
			});
		}

		// Helpers

		private async UniTask LoadSettings()
		{
			Message.Value = "Loading settings...";
			await m_Preferences.Load();
		}
	}
}
