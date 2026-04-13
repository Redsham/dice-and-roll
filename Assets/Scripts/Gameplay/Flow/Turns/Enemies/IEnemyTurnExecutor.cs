using System.Threading;
using Cysharp.Threading.Tasks;

namespace Gameplay.Flow.Turns.Enemies
{
	public interface IEnemyTurnExecutor
	{
		UniTask ExecuteAsync(CancellationToken cancellationToken);
	}
}
