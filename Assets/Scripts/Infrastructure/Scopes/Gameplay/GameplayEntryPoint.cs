using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Flow.Scenario;
using VContainer.Unity;


namespace Infrastructure.Scopes.Gameplay
{
	public sealed class GameplayEntryPoint : IAsyncStartable
	{
		private readonly IGameplayScenario m_GameplayScenario;

		public GameplayEntryPoint(IGameplayScenario gameplayScenario)
		{
			m_GameplayScenario = gameplayScenario;
		}

		public async UniTask StartAsync(CancellationToken cancellation)
		{
			await m_GameplayScenario.RunAsync(cancellation);
		}
	}
}