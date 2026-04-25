using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Composition;
using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.Runtime;
using Gameplay.Flow.Spawning.Effects;
using Gameplay.World.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Gameplay.Flow.Spawning.Runtime
{
	public sealed class EnemySpawnEffectPlayer
	{
		private readonly GameplaySceneConfiguration m_Configuration;
		private readonly INavigationService         m_NavigationService;
		private readonly EnemyService               m_EnemyService;

		public EnemySpawnEffectPlayer(
			GameplaySceneConfiguration configuration,
			INavigationService         navigationService,
			EnemyService               enemyService
		)
		{
			m_Configuration     = configuration;
			m_NavigationService = navigationService;
			m_EnemyService      = enemyService;
		}

		public void Clear()
		{
			m_EnemyService.Clear();
		}

		public async UniTask<EnemyRuntimeHandle> SpawnAsync(
			EnemyBehaviour enemyPrefab,
			Vector2Int     cell,
			GameObject     spawnEffectPrefab,
			CancellationToken cancellationToken
		)
		{
			if (spawnEffectPrefab == null) {
				return await m_EnemyService.SpawnAsync(enemyPrefab, cell, cancellationToken);
			}

			GameObject spawnEffectInstance = Object.Instantiate(spawnEffectPrefab, m_Configuration.ActorParent);
			EnemySpawnEffectBehaviour spawnEffect = spawnEffectInstance.GetComponentInChildren<EnemySpawnEffectBehaviour>(true);
			if (spawnEffect == null) {
				Object.Destroy(spawnEffectInstance);
				throw new InvalidOperationException($"Spawn effect prefab '{spawnEffectPrefab.name}' must contain an {nameof(EnemySpawnEffectBehaviour)} component.");
			}

			EnemySpawnEffectContext context = new(
			                                     cell,
			                                     m_NavigationService.Basis.GetCellCenter(cell),
			                                     m_NavigationService.Basis,
			                                     m_Configuration.ActorParent
			                                    );

			await spawnEffect.PlayAsync(context, cancellationToken);
			return await m_EnemyService.SpawnAsync(enemyPrefab, cell, playSpawnAnimation: false, cancellationToken);
		}
	}
}
