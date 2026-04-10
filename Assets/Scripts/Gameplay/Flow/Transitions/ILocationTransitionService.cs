using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Transitions
{
	public interface ILocationTransitionService
	{
		UniTask PlaySwapAsync(CancellationToken cancellationToken);
	}
}
