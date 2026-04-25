using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Enemies.Authoring;
using Gameplay.Levels.Authoring;
using Gameplay.Levels.Authoring.Enemies;
using Gameplay.Levels.Runtime;
using Gameplay.Navigation;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Flow.Spawning.Runtime
{
	public sealed class RandomEnemySpawner : IEnemySpawner
	{
		private readonly LevelService       m_LevelService;
		private readonly IGameCameraController m_GameCameraController;
		private readonly INavigationService m_NavigationService;
		private readonly DiceService        m_PlayerService;
		private readonly EnemySpawnEffectPlayer m_SpawnEffectPlayer;
		private const float CAMERA_RETURN_BLEND_DURATION = 0.85f;

		public RandomEnemySpawner(
			LevelService            levelService,
			IGameCameraController   gameCameraController,
			INavigationService      navigationService,
			DiceService             playerService,
			EnemySpawnEffectPlayer  spawnEffectPlayer
		)
		{
			m_LevelService         = levelService;
			m_GameCameraController = gameCameraController;
			m_NavigationService    = navigationService;
			m_PlayerService        = playerService;
			m_SpawnEffectPlayer    = spawnEffectPlayer;
		}

		public async UniTask SpawnAsync(CancellationToken cancellationToken)
		{
			m_SpawnEffectPlayer.Clear();

			LevelBehaviour              level   = m_LevelService.CurrentLevel;
			RandomEnemySpawnerAuthoring spawner = level != null ? level.GetComponentInChildren<RandomEnemySpawnerAuthoring>(true) : null;
			if (spawner == null || spawner.EnemyPrefabs == null || spawner.EnemyPrefabs.Length == 0 || spawner.SpawnCount <= 0) {
				return;
			}

			List<Vector2Int> availableCells = CollectAvailableCells(level.NavGrid, m_PlayerService.Position);
			int              spawnCount     = Mathf.Min(spawner.SpawnCount, availableCells.Count);

			m_PlayerService.SuppressShotPreview();

			try {
				if (spawner.InitialSpawnDelay > 0f) {
					await UniTask.Delay(System.TimeSpan.FromSeconds(spawner.InitialSpawnDelay), cancellationToken: cancellationToken);
				}

				for (int i = 0; i < spawnCount; i++) {
					int        cellIndex = Random.Range(0, availableCells.Count);
					Vector2Int cell      = availableCells[cellIndex];
					availableCells.RemoveAt(cellIndex);

					EnemyBehaviour enemyPrefab = spawner.EnemyPrefabs[Random.Range(0, spawner.EnemyPrefabs.Length)];
					if (enemyPrefab == null) {
						Debug.LogWarning("RandomEnemySpawner encountered a null enemy prefab reference and skipped it.", spawner);
						continue;
					}

					await m_SpawnEffectPlayer.SpawnAsync(enemyPrefab, cell, spawner.SpawnEffectPrefab, cancellationToken);

					if (i < spawnCount - 1 && spawner.DelayBetweenSpawns > 0f) {
						await UniTask.Delay(System.TimeSpan.FromSeconds(spawner.DelayBetweenSpawns), cancellationToken: cancellationToken);
					}
				}
			}
			finally {
				m_PlayerService.ReleaseShotPreviewSuppression();

				if (m_PlayerService.PlayerObject != null) {
					m_GameCameraController.FollowWithOrbit(m_PlayerService.PlayerObject.transform, CAMERA_RETURN_BLEND_DURATION);
				}
			}
		}

		private List<Vector2Int> CollectAvailableCells(NavGrid grid, Vector2Int playerCell)
		{
			List<Vector2Int> result = new();

			for (int y = 0; y < grid.Height; y++) {
				for (int x = 0; x < grid.Width; x++) {
					Vector2Int cell = new(x, y);
					if (cell == playerCell) {
						continue;
					}

					if (m_NavigationService.CanOccupy(cell)) {
						result.Add(cell);
					}
				}
			}

			return result;
		}
	}
}
