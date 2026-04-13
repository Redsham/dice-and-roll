using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Composition;
using Gameplay.Enemies.Authoring;
using Gameplay.Flow.GameState;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;


namespace Gameplay.Enemies.Runtime
{
	public sealed class EnemyService
	{
		private const float ACTOR_HEIGHT_OFFSET = 0.5f;

		// === Dependencies ===

		private readonly GameplaySceneConfiguration m_Configuration;
		private readonly IObjectResolver            m_ObjectResolver;
		private readonly INavigationService         m_NavigationService;
		private readonly DiceService                m_PlayerService;
		private readonly IGameplayStateService      m_GameplayStateService;
		private readonly LevelNodeService           m_LevelNodeService;

		// === Runtime ===

		private readonly List<EnemyRuntimeHandle> m_Enemies = new();

		public EnemyService(
			GameplaySceneConfiguration configuration,
			IObjectResolver            objectResolver,
			INavigationService         navigationService,
			DiceService                playerService,
			IGameplayStateService      gameplayStateService,
			LevelNodeService           levelNodeService
		)
		{
			m_Configuration         = configuration;
			m_ObjectResolver        = objectResolver;
			m_NavigationService     = navigationService;
			m_PlayerService         = playerService;
			m_GameplayStateService  = gameplayStateService;
			m_LevelNodeService      = levelNodeService;
		}

		public int AliveCount
		{
			get
			{
				int count = 0;
				for (int i = 0; i < m_Enemies.Count; i++) {
					if (m_Enemies[i].IsAlive) {
						count++;
					}
				}

				return count;
			}
		}

		public IReadOnlyList<EnemyRuntimeHandle> Enemies => m_Enemies;

		public void Clear()
		{
			for (int i = 0; i < m_Enemies.Count; i++) {
				if (m_Enemies[i].IsAlive) {
					m_LevelNodeService.NotifyActorLeft(m_Enemies[i].State.Position, m_Enemies[i].Behaviour.gameObject);
					m_NavigationService.TryClearEntity(m_Enemies[i].Cell, m_Enemies[i]);
					Object.Destroy(m_Enemies[i].Behaviour.gameObject);
				}
			}

			m_Enemies.Clear();
		}

		public async UniTask<EnemyRuntimeHandle> SpawnAsync(EnemyBehaviour prefab, Vector2Int cell, CancellationToken cancellationToken)
		{
			// === Validation ===

			if (prefab == null) {
				throw new InvalidOperationException("EnemyService cannot spawn a null enemy prefab.");
			}

			EnemyBehaviour enemyBehaviour = m_ObjectResolver.Instantiate(prefab, m_Configuration.ActorParent);
			GridBasis basis = m_NavigationService.Basis;
			enemyBehaviour.transform.SetPositionAndRotation(
			                                                basis.GetCellCenter(cell) + basis.Up * ACTOR_HEIGHT_OFFSET,
			                                                Quaternion.identity
			                                               );

			OverrideSpawnCell(enemyBehaviour, cell);

			EnemyRuntimeHandle handle = new(enemyBehaviour, m_PlayerService, m_NavigationService, m_LevelNodeService);
			m_Enemies.Add(handle);
			m_NavigationService.TrySetEntity(handle.Cell, handle);
			await handle.SpawnAsync(cancellationToken);
			return handle;
		}

		public async UniTask ExecuteTurnAsync(CancellationToken cancellationToken)
		{
			for (int i = m_Enemies.Count - 1; i >= 0; i--) {
				if (!m_Enemies[i].IsAlive) {
					m_Enemies.RemoveAt(i);
				}
			}

			for (int i = 0; i < m_Enemies.Count; i++) {
				if (!m_Enemies[i].IsAlive) {
					continue;
				}

				await m_Enemies[i].ExecuteTurnAsync(cancellationToken);
				if (m_GameplayStateService.HasEnded) {
					return;
				}
			}

			if (AliveCount == 0) {
				m_GameplayStateService.End(GameplayEndReason.LevelCleared);
			}
		}

		private static void OverrideSpawnCell(EnemyBehaviour enemyBehaviour, Vector2Int cell)
		{
			enemyBehaviour.SetGridPosition(cell);
		}
	}
}
