using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Spawning
{
	public sealed class NullEnemySpawner : IEnemySpawner
	{
		public UniTask SpawnAsync(CancellationToken cancellationToken)
		{
			return UniTask.CompletedTask;
		}
	}
}