using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Services;
using Infrastructure.Services.Scenes;
using VContainer.Unity;


namespace Infrastructure.Scopes.Root
{
	public class RootEntryPoint : IAsyncStartable
	{
		private readonly BootstrapService m_BootstrapService;
		private readonly SceneService     m_SceneService;

		public RootEntryPoint(SceneService sceneService, BootstrapService bootstrapService)
		{
			m_BootstrapService = bootstrapService;
			m_SceneService     = sceneService;
		}

		public async UniTask StartAsync(CancellationToken cancellation = new())
		{
			string startupScene = m_SceneService.GetActiveScene();
			bool   isBootstrap  = startupScene == "Bootstrap";

			if (!isBootstrap) {
				await m_SceneService.LoadSceneAsync("Bootstrap");
			}

			await UniTask.WaitUntil(() => m_BootstrapService.Initialized, cancellationToken: cancellation);

			if (isBootstrap) await m_BootstrapService.LoadMenu();
			else await m_BootstrapService.LoadScene(startupScene);
		}
	}
}