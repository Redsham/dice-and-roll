using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Gameplay.Flow.Spawning.Effects
{
	public abstract class EnemySpawnEffectBehaviour : MonoBehaviour, IEnemySpawnEffect
	{
		public abstract UniTask PlayAsync(EnemySpawnEffectContext context, CancellationToken cancellationToken);
	}
}
