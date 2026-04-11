using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Input
{
	public interface IPlayerTurnSource
	{
		UniTask<PlayerTurnCommand> WaitForTurnAsync(CancellationToken cancellationToken);
	}
}