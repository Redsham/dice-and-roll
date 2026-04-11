using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.Authoring;
using UnityEngine;


namespace Gameplay.Enemies.Runtime
{
	public interface IEnemyService
	{
		int AliveCount { get; }
		IReadOnlyList<EnemyRuntimeHandle> Enemies { get; }

		void Clear();
		UniTask<EnemyRuntimeHandle> SpawnAsync(EnemyBehaviour prefab, Vector2Int cell, CancellationToken cancellationToken);
		UniTask ExecuteTurnAsync(CancellationToken cancellationToken);
	}
}
