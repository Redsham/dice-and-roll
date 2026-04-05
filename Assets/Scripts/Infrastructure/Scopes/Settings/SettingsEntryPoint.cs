using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Services;
using Settings.Views;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes.Settings
{
	public class SettingsEntryPoint : IAsyncStartable
	{
		[Inject] private readonly PreferencesService m_Preferences;
		[Inject] private readonly SettingsView       n_View;
		
		public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
		{
			await UniTask.WaitUntil(() => m_Preferences.IsReady, cancellationToken: cancellation);
			n_View.Init();
		}
	}
}