using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Player.Domain;


namespace Gameplay.Flow.Input
{
	public interface IPlayerTurnSource
	{
		UniTask<RollDirection> WaitForTurnAsync(CancellationToken cancellationToken);
	}
}
