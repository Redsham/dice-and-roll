using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Flow.Turns;


namespace Gameplay.Flow.Loop
{
	public sealed class DefaultGameplayLoop : IGameplayLoop
	{
		private readonly IGameTurn m_PlayerTurn;

		public DefaultGameplayLoop(IGameTurn playerTurn)
		{
			m_PlayerTurn = playerTurn;
		}

		public async UniTask RunAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				await m_PlayerTurn.ExecuteAsync(cancellationToken);
			}
		}
	}
}
