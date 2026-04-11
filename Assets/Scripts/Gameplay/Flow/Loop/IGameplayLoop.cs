using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Loop
{
	public interface IGameplayLoop
	{
		UniTask RunAsync(CancellationToken cancellationToken);
	}
}