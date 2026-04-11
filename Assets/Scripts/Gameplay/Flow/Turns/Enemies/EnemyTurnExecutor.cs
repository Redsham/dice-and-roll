using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.Runtime;


namespace Gameplay.Flow.Turns.Enemies
{
	public sealed class EnemyTurnExecutor : IEnemyTurnExecutor
	{
		private readonly IEnemyService m_EnemyService;

		public EnemyTurnExecutor(IEnemyService enemyService)
		{
			m_EnemyService = enemyService;
		}

		public async UniTask ExecuteAsync(CancellationToken cancellationToken)
		{
			await m_EnemyService.ExecuteTurnAsync(cancellationToken);
		}
	}
}
