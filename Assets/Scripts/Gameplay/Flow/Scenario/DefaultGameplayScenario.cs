using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Composition;
using Gameplay.Enemies.Runtime;
using Gameplay.Flow.GameState;
using Gameplay.Flow.Loop;
using Gameplay.Flow.Sequences;
using Gameplay.Flow.Spawning;
using Gameplay.Flow.Transitions;
using Gameplay.Levels.Authoring;
using Gameplay.Levels.Data;
using Gameplay.Levels.Runtime;
using Gameplay.Player.Authoring;
using Gameplay.Player.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Gameplay.Flow.Scenario
{
	public sealed class DefaultGameplayScenario : IGameplayScenario
	{
		private readonly GameplaySceneConfiguration m_Configuration;
		private readonly ILevelSequence             m_LevelSequence;
		private readonly ILevelService              m_LevelService;
		private readonly IObjectResolver            m_ObjectResolver;
		private readonly IPlayerService             m_PlayerService;
		private readonly IGameCameraController      m_GameCameraController;
		private readonly IEnemySpawner              m_EnemySpawner;
		private readonly IEnemyService              m_EnemyService;
		private readonly IGameplayLoop              m_GameplayLoop;
		private readonly IGameplayStateService      m_GameplayStateService;
		private readonly ILocationTransitionService m_LocationTransition;

		public DefaultGameplayScenario(
			GameplaySceneConfiguration configuration,
			ILevelSequence             levelSequence,
			ILevelService              levelService,
			IObjectResolver            objectResolver,
			IPlayerService             playerService,
			IGameCameraController      gameCameraController,
			IEnemySpawner              enemySpawner,
			IEnemyService              enemyService,
			IGameplayLoop              gameplayLoop,
			IGameplayStateService      gameplayStateService,
			ILocationTransitionService locationTransition
		)
		{
			m_Configuration        = configuration;
			m_LevelSequence        = levelSequence;
			m_LevelService         = levelService;
			m_ObjectResolver       = objectResolver;
			m_PlayerService        = playerService;
			m_GameCameraController = gameCameraController;
			m_EnemySpawner         = enemySpawner;
			m_EnemyService         = enemyService;
			m_GameplayLoop         = gameplayLoop;
			m_GameplayStateService = gameplayStateService;
			m_LocationTransition   = locationTransition;
		}

		public async UniTask RunAsync(CancellationToken cancellationToken)
		{
			LevelAsset levelAsset = await m_LevelSequence.GetFirstAsync(cancellationToken);

			while (!cancellationToken.IsCancellationRequested) {
				m_EnemyService.Clear();
				m_PlayerService.ClearPlayer();

				await m_LevelService.ReplaceAsync(levelAsset, cancellationToken);

				SpawnAndBindPlayer();

				await m_EnemySpawner.SpawnAsync(cancellationToken);
				await m_GameplayLoop.RunAsync(cancellationToken);

				if (m_GameplayStateService.EndReason == GameplayEndReason.PlayerDefeated) {
					break;
				}

				await m_LocationTransition.PlaySwapAsync(cancellationToken);
				levelAsset = await m_LevelSequence.GetNextAsync(cancellationToken);
			}
		}

		private void SpawnAndBindPlayer()
		{
			LevelBehaviour level         = m_LevelService.CurrentLevel;
			DiceBehaviour  player        = m_ObjectResolver.Instantiate(m_Configuration.PlayerPrefab, m_Configuration.ActorParent);
			Vector2Int     startPosition = level.PlayerStart.GridPosition;
			m_PlayerService.BindPlayer(player, startPosition);
			m_GameCameraController.FollowWithOrbit(player.transform);
		}
	}
}