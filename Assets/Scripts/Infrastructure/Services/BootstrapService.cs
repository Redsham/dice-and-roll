using Audio.UI;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Scenes;
using R3;
using UI.Effects;
using VContainer;


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
		public bool Initialized { get; private set; } = false;
		/// <summary>
		/// Human-readable bootstrap progress message.
		/// </summary>
		public ReactiveProperty<string> Message { get; } = new();

		// Dependencies

		[Inject] private readonly SceneService       m_SceneService;
		[Inject] private readonly UIFade             m_Fade;
		[Inject] private readonly PreferencesService m_Preferences;
		[Inject] private readonly AudioMixerService  m_AudioMixerService;
		[Inject] private readonly UISounds           m_UISounds;

		// Lifecycle

		/// <summary>
		/// Initializes bootstrap-time systems.
		/// </summary>
		public async UniTask Initialize()
		{
			await LoadSettings();
			await LoadUISounds();
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

		private async UniTask LoadUISounds()
		{
			Message.Value = "Loading UI sounds...";
			await m_UISounds.Init();
		}
	}
}
