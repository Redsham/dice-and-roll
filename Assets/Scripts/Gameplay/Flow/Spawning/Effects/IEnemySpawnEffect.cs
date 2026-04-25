using System.Threading;
using Cysharp.Threading.Tasks;


namespace Gameplay.Flow.Spawning.Effects
{
	public interface IEnemySpawnEffect
	{
		UniTask PlayAsync(EnemySpawnEffectContext context, CancellationToken cancellationToken);
	}
}
