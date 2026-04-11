using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Transitions
{
	public sealed class NullLocationTransitionService : ILocationTransitionService
	{
		public UniTask PlaySwapAsync(CancellationToken cancellationToken)
		{
			return UniTask.CompletedTask;
		}
	}
}