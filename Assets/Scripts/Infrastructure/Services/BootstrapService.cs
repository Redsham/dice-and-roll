using Cysharp.Threading.Tasks;
using Infrastructure.Services.Scenes;
using R3;
using UI.Effects;


namespace Infrastructure.Services
{
	public class BootstrapService
	{
		public bool                     Initialized { get; private set; } = false;
		public ReactiveProperty<string> Message     { get; }              = new();
		
		private readonly SceneService m_SceneService;
		private readonly UIFade       m_Fade;

		
		public BootstrapService(SceneService sceneService, UIFade fade)
		{
			m_SceneService = sceneService;
			m_Fade = fade;
		}
		
		
		public async UniTask Initialize()
		{
			// Load settings
			await LoadSettings();
			
			Initialized = true;
		}

		public async UniTask LoadScene(string sceneName)
		{
			Message.Value = "Loading scene...";
			await m_SceneService.LoadSceneAsync(sceneName);
		}
		public async UniTask LoadMenu()
		{
			Message.Value = "Loading menu...";
			await m_Fade.ActionAsync(async () => {
				await UniTask.WaitForSeconds(0.25f);
				await m_SceneService.LoadSceneAsync("Menu");
			});
		}

		private async UniTask LoadSettings()
		{
			Message.Value = "Loading settings...";
			await UniTask.WaitForSeconds(1.0f);
		}
	}
}