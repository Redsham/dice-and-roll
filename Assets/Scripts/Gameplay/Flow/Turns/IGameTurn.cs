using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Turns
{
	public interface IGameTurn
	{
		UniTask ExecuteAsync(CancellationToken cancellationToken);
	}
}
