using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
		private readonly INavigationService m_NavigationService;
		private readonly DiceService        m_PlayerService;
		private readonly EnemySpawnEffectPlayer m_SpawnEffectPlayer;

		public RandomEnemySpawner(
			LevelService            levelService,
			INavigationService      navigationService,
			DiceService             playerService,
			EnemySpawnEffectPlayer  spawnEffectPlayer
		)
		{
			m_LevelService      = levelService;
			m_NavigationService = navigationService;
			m_PlayerService     = playerService;
			m_SpawnEffectPlayer = spawnEffectPlayer;
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
