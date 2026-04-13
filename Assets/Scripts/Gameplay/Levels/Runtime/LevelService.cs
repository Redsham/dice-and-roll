using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Composition;
using Gameplay.Levels.Authoring;
using Gameplay.Levels.Data;
using Gameplay.Nodes.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;


namespace Gameplay.Levels.Runtime
{
	public sealed class LevelService
	{
		private readonly GameplaySceneConfiguration        m_Configuration;
		private readonly IObjectResolver                   m_ObjectResolver;
		private readonly INavigationService                m_NavigationService;
		private readonly LevelNodeService                  m_LevelNodeService;
		private          AsyncOperationHandle<GameObject>? m_CurrentLevelHandle;

		public LevelService(
			GameplaySceneConfiguration configuration,
			IObjectResolver            objectResolver,
			INavigationService         navigationService,
			LevelNodeService           levelNodeService
		)
		{
			m_Configuration         = configuration;
			m_ObjectResolver        = objectResolver;
			m_NavigationService     = navigationService;
			m_LevelNodeService      = levelNodeService;
		}

		public LevelAsset     CurrentAsset { get; private set; }
		public LevelBehaviour CurrentLevel { get; private set; }
		public bool           HasLevel     => CurrentLevel != null;

		public async UniTask<LevelBehaviour> LoadAsync(LevelAsset levelAsset, CancellationToken cancellationToken)
		{
			if (levelAsset == null) {
				throw new ArgumentNullException(nameof(levelAsset));
			}

			if (levelAsset.LevelPrefab == null || !levelAsset.LevelPrefab.RuntimeKeyIsValid()) {
				throw new InvalidOperationException($"LevelAsset '{levelAsset.name}' does not reference a level prefab.");
			}

			CurrentAsset = levelAsset;
			AsyncOperationHandle<GameObject> handle = levelAsset.LevelPrefab.InstantiateAsync(m_Configuration.LevelParent);
			m_CurrentLevelHandle = handle;

			GameObject levelObject = await handle.ToUniTask(cancellationToken: cancellationToken);
			m_ObjectResolver.InjectGameObject(levelObject);
			CurrentLevel = levelObject.GetComponent<LevelBehaviour>();

			if (CurrentLevel == null) {
				Addressables.ReleaseInstance(levelObject);
				m_CurrentLevelHandle = null;
				CurrentAsset         = null;
				throw new InvalidOperationException(
				                                    $"Addressable level '{levelAsset.name}' must contain a {nameof(LevelBehaviour)} component on the root object."
				                                   );
			}

			CurrentLevel.Initialize();
			m_NavigationService.BindLevel(CurrentLevel);
			m_LevelNodeService.BindLevel(CurrentLevel.NavGrid, CurrentLevel.GetTileBehaviours());

			return CurrentLevel;
		}

		public async UniTask ReplaceAsync(LevelAsset levelAsset, CancellationToken cancellationToken)
		{
			if (CurrentLevel != null) {
				GameObject levelObject = CurrentLevel.gameObject;
				CurrentLevel = null;
				CurrentAsset = null;
				m_NavigationService.ClearLevel();
				m_LevelNodeService.ClearLevel();
				if (m_CurrentLevelHandle.HasValue) {
					Addressables.ReleaseInstance(levelObject);
					m_CurrentLevelHandle = null;
				}
			}

			await LoadAsync(levelAsset, cancellationToken);
		}
	}
}
