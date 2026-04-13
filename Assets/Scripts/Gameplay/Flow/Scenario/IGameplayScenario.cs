using System.Threading;
using Cysharp.Threading.Tasks;

namespace Gameplay.Flow.Scenario
{
	public interface IGameplayScenario
	{
		UniTask RunAsync(CancellationToken cancellationToken);
	}
}
