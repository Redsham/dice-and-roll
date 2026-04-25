using System;
using System.Collections.Generic;
using System.Linq;
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
		private          int                      m_AliveCount;
		private          int                      m_SpawnedCount;
		private          int                      m_DiedCount;
		private          int?                     m_PlannedEnemyCount;

		// === Events ===

		public event Action<EnemyRuntimeHandle> OnSpawned         = delegate { };
		public event Action<EnemyRuntimeHandle> OnDied            = delegate { };
		public event Action                     OnTrackingChanged = delegate { };

		public EnemyService(
			GameplaySceneConfiguration configuration, IObjectResolver       objectResolver,       INavigationService navigationService,
			DiceService                playerService, IGameplayStateService gameplayStateService, LevelNodeService   levelNodeService)
		{
			m_Configuration        = configuration;
			m_ObjectResolver       = objectResolver;
			m_NavigationService    = navigationService;
			m_PlayerService        = playerService;
			m_GameplayStateService = gameplayStateService;
			m_LevelNodeService     = levelNodeService;
		}

		public int  AliveCount                 => m_AliveCount;
		public int  SpawnedCount               => m_SpawnedCount;
		public int  DiedCount                  => m_DiedCount;
		public int? PlannedEnemyCount          => m_PlannedEnemyCount;
		public bool HasFinitePlannedEnemyCount => m_PlannedEnemyCount.HasValue;
		public bool IsEncounterCleared         => HasFinitePlannedEnemyCount && m_AliveCount == 0 && m_DiedCount >= m_PlannedEnemyCount.Value;

		public IReadOnlyList<EnemyRuntimeHandle> Enemies => m_Enemies;

		public void Clear()
		{
			foreach (EnemyRuntimeHandle t in m_Enemies) {
				t.OnDied -= HandleEnemyDied;
				if (!t.IsAlive) {
					continue;
				}

				m_LevelNodeService.NotifyActorLeft(t.State.Position, t.Behaviour.gameObject);
				m_NavigationService.TryClearEntity(t.Cell, t);
				Object.Destroy(t.Behaviour.gameObject);
			}

			m_Enemies.Clear();
			m_AliveCount        = 0;
			m_SpawnedCount      = 0;
			m_DiedCount         = 0;
			m_PlannedEnemyCount = 0;
			OnTrackingChanged.Invoke();
		}

		public EnemyRuntimeHandle Spawn(EnemyBehaviour prefab, Vector2Int cell)
		{
			// === Validation ===

			if (prefab == null) {
				throw new InvalidOperationException("EnemyService cannot spawn a null enemy prefab.");
			}

			EnemyBehaviour enemyBehaviour = m_ObjectResolver.Instantiate(prefab, m_Configuration.ActorParent);
			GridBasis      basis          = m_NavigationService.Basis;
			enemyBehaviour.transform.SetPositionAndRotation(
			                                                basis.GetCellCenter(cell) + basis.Up * ACTOR_HEIGHT_OFFSET,
			                                                Quaternion.identity
			                                               );

			OverrideSpawnCell(enemyBehaviour, cell);

			EnemyRuntimeHandle handle = new(enemyBehaviour, m_PlayerService, m_NavigationService, m_LevelNodeService);

			m_Enemies.Add(handle);
			handle.OnDied += HandleEnemyDied;

			m_NavigationService.TrySetEntity(handle.Cell, handle);
			handle.Spawn();

			m_AliveCount++;
			m_SpawnedCount++;

			OnSpawned.Invoke(handle);

			return handle;
		}

		public void SetPlannedEnemyCount(int? count)
		{
			m_PlannedEnemyCount = count.HasValue ? Mathf.Max(0, count.Value) : null;
			OnTrackingChanged.Invoke();
		}

		public void AddPlannedEnemyCount(int count)
		{
			if (count <= 0 || !m_PlannedEnemyCount.HasValue) {
				return;
			}

			m_PlannedEnemyCount += count;
			OnTrackingChanged.Invoke();
		}

		public void RemovePlannedEnemyCount(int count)
		{
			if (count <= 0 || !m_PlannedEnemyCount.HasValue) {
				return;
			}

			m_PlannedEnemyCount = Mathf.Max(0, m_PlannedEnemyCount.Value - count);
			OnTrackingChanged.Invoke();
		}

		public async UniTask ExecuteTurnAsync(CancellationToken cancellationToken)
		{
			for (int i = m_Enemies.Count - 1; i >= 0; i--) {
				if (!m_Enemies[i].IsAlive) {
					m_Enemies[i].OnDied -= HandleEnemyDied;
					m_Enemies.RemoveAt(i);
				}
			}

			foreach (EnemyRuntimeHandle t in m_Enemies.Where(t => t.IsAlive)) {
				await t.ExecuteTurnAsync(cancellationToken);
				if (m_GameplayStateService.HasEnded) return;
			}

			if (IsEncounterCleared) {
				m_GameplayStateService.End(GameplayEndReason.LevelCleared);
			}
		}

		private void HandleEnemyDied(EnemyRuntimeHandle handle)
		{
			m_AliveCount = Mathf.Max(0, m_AliveCount - 1);
			m_DiedCount++;
			OnDied.Invoke(handle);
		}

		private static void OverrideSpawnCell(EnemyBehaviour enemyBehaviour, Vector2Int cell)
		{
			enemyBehaviour.SetGridPosition(cell);
		}
	}
}