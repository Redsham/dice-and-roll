using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Flow.Input;
using Gameplay.Player.Runtime;


namespace Gameplay.Flow.Turns
{
	public sealed class PlayerCommandTurn : IGameTurn
	{
		private readonly IPlayerTurnSource m_TurnSource;
		private readonly IPlayerService    m_PlayerService;

		public PlayerCommandTurn(IPlayerTurnSource turnSource, IPlayerService playerService)
		{
			m_TurnSource = turnSource;
			m_PlayerService = playerService;
		}

		public async UniTask ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				PlayerTurnCommand command = await m_TurnSource.WaitForTurnAsync(cancellationToken);
				if (await TryExecuteAsync(command)) {
					return;
				}
			}
		}

		private UniTask<bool> TryExecuteAsync(PlayerTurnCommand command)
		{
			return command.Type switch
			{
				PlayerTurnCommandType.Move => m_PlayerService.TryRollAsync(command.MoveDirection),
				PlayerTurnCommandType.Shoot => m_PlayerService.TryShootAsync(command.AimPoint),
				_ => UniTask.FromResult(false)
			};
		}
	}
}
