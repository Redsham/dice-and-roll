using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Composition;
using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.Runtime;
using Gameplay.Flow.Spawning.Effects;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;


namespace Gameplay.Flow.Spawning.Runtime
{
	public sealed class EnemySpawnEffectPlayer
	{
		private const float CAMERA_FOCUS_BLEND_DURATION = 0.5f;

		private readonly GameplaySceneConfiguration m_Configuration;
		private readonly IObjectResolver            m_ObjectResolver;
		private readonly IGameCameraController      m_GameCameraController;
		private readonly INavigationService         m_NavigationService;
		private readonly DiceService                m_PlayerService;
		private readonly EnemyService               m_EnemyService;

		public EnemySpawnEffectPlayer(
			GameplaySceneConfiguration configuration,
			IObjectResolver            objectResolver,
			IGameCameraController      gameCameraController,
			INavigationService         navigationService,
			DiceService                playerService,
			EnemyService               enemyService
		)
		{
			m_Configuration        = configuration;
			m_ObjectResolver       = objectResolver;
			m_GameCameraController = gameCameraController;
			m_NavigationService    = navigationService;
			m_PlayerService        = playerService;
			m_EnemyService         = enemyService;
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
				return m_EnemyService.Spawn(enemyPrefab, cell);
			}

			EnemySpawnEffectBehaviour spawnEffect = InstantiateSpawnEffect(spawnEffectPrefab);
			EnemySpawnEffectContext   context     = CreateContext(cell);
			spawnEffect.Prepare(context);
			FocusCamera(spawnEffect, context);
			Action unsubscribeFieldOfView = BindCameraFieldOfView(spawnEffect);

			try {
				await spawnEffect.PlayAsync(context, cancellationToken);
			}
			finally {
				unsubscribeFieldOfView?.Invoke();
			}

			return m_EnemyService.Spawn(enemyPrefab, cell);
		}

		private EnemySpawnEffectBehaviour InstantiateSpawnEffect(GameObject spawnEffectPrefab)
		{
			GameObject effectInstance = m_ObjectResolver.Instantiate(spawnEffectPrefab, m_Configuration.ActorParent);
			EnemySpawnEffectBehaviour spawnEffect = effectInstance.GetComponentInChildren<EnemySpawnEffectBehaviour>(true);
			if (spawnEffect != null) {
				return spawnEffect;
			}

			Object.Destroy(effectInstance);
			throw new InvalidOperationException($"Spawn effect prefab '{spawnEffectPrefab.name}' must contain an {nameof(EnemySpawnEffectBehaviour)} component.");
		}

		private EnemySpawnEffectContext CreateContext(Vector2Int cell)
		{
			return new(
				cell,
				m_NavigationService.Basis.GetCellCenter(cell),
				m_NavigationService.Basis,
				m_Configuration.ActorParent
			);
		}

		private void FocusCamera(EnemySpawnEffectBehaviour spawnEffect, in EnemySpawnEffectContext context)
		{
			Transform playerTransform = m_PlayerService.PlayerObject != null ? m_PlayerService.PlayerObject.transform : null;
			if (playerTransform == null) {
				return;
			}

			Transform focusTarget = spawnEffect.GetCameraFocusTarget();
			if (focusTarget != null) {
				m_GameCameraController.FocusOnTrackedTransform(playerTransform, focusTarget, CAMERA_FOCUS_BLEND_DURATION);
				return;
			}

			m_GameCameraController.FocusOnWorldPoint(playerTransform, context.TargetPosition, CAMERA_FOCUS_BLEND_DURATION);
		}

		private Action BindCameraFieldOfView(EnemySpawnEffectBehaviour spawnEffect)
		{
			if (!spawnEffect.TryGetCameraFieldOfViewProfile(out SpawnEffectCameraFieldOfViewProfile profile)) {
				return null;
			}

			void HandleProgress(float progress)
			{
				float fieldOfView = Mathf.Lerp(profile.StartFieldOfView, profile.EndFieldOfView, progress);
				m_GameCameraController.SetFieldOfView(fieldOfView, profile.SmoothTime);
			}

			spawnEffect.CameraFieldOfViewProgressChanged += HandleProgress;
			HandleProgress(0.0f);
			return () => spawnEffect.CameraFieldOfViewProgressChanged -= HandleProgress;
		}
	}
}
