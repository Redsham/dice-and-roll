using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Spawning
{
	public interface IEnemySpawner
	{
		UniTask SpawnAsync(CancellationToken cancellationToken);
	}
}