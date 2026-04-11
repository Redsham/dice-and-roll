using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Flow.GameState;
using Gameplay.Flow.Turns;
using Gameplay.Flow.Turns.Enemies;


namespace Gameplay.Flow.Loop
{
	public sealed class DefaultGameplayLoop : IGameplayLoop
	{
		private readonly IGameTurn             m_PlayerTurn;
		private readonly IEnemyTurnExecutor    m_EnemyTurnExecutor;
		private readonly IGameplayStateService m_GameplayStateService;

		public DefaultGameplayLoop(IGameTurn playerTurn, IEnemyTurnExecutor enemyTurnExecutor, IGameplayStateService gameplayStateService)
		{
			m_PlayerTurn           = playerTurn;
			m_EnemyTurnExecutor    = enemyTurnExecutor;
			m_GameplayStateService = gameplayStateService;
		}

		public async UniTask RunAsync(CancellationToken cancellationToken)
		{
			m_GameplayStateService.Begin();

			while (!cancellationToken.IsCancellationRequested && !m_GameplayStateService.HasEnded) {
				await m_PlayerTurn.ExecuteAsync(cancellationToken);
				if (m_GameplayStateService.HasEnded) {
					break;
				}

				await m_EnemyTurnExecutor.ExecuteAsync(cancellationToken);
				if (!m_GameplayStateService.HasEnded) {
					m_GameplayStateService.AdvanceTurn();
				}
			}
		}
	}
}
