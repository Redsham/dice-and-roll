using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Flow.Input;
using Gameplay.Player.Domain;
using Gameplay.Player.Runtime;


namespace Gameplay.Flow.Turns
{
	public sealed class PlayerMovementTurn : IGameTurn
	{
		private readonly IPlayerTurnSource m_TurnSource;
		private readonly IPlayerService    m_PlayerService;

		public PlayerMovementTurn(IPlayerTurnSource turnSource, IPlayerService playerService)
		{
			m_TurnSource = turnSource;
			m_PlayerService = playerService;
		}

		public async UniTask ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				RollDirection direction = await m_TurnSource.WaitForTurnAsync(cancellationToken);
				if (await m_PlayerService.TryRollAsync(direction)) {
					return;
				}
			}
		}
	}
}
