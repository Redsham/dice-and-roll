using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Infrastructure.Services.Scenes
{
	public class SceneService
	{
		/// List of hooks to notify about scene transition progress.
		private readonly List<ISceneTransitionHook> m_Hooks = new();

		/// Adds a hook to be notified about scene transition progress.
		public void AddHook(ISceneTransitionHook hook)
		{
			m_Hooks.Add(hook);
			Debug.Log($"[{nameof(SceneService)}] Hook added: {hook.GetType().Name}");
		}
		/// Removes a hook from being notified about scene transition progress.
		public void RemoveHook(ISceneTransitionHook hook)
		{
			m_Hooks.Remove(hook);
			Debug.Log($"[{nameof(SceneService)}] Hook removed: {hook.GetType().Name}");
		}
		
		/// Loads a scene asynchronously and reports progress through the provided observer.
		public async UniTask LoadSceneAsync(string sceneName, IProgress<SceneTransitionProgress> progress = null)
		{
			Debug.Log($"[{nameof(SceneService)}] Started loading scene: {sceneName}");
			
			// Start loading the scene asynchronously
			AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			if (loadOperation == null)
				throw new InvalidOperationException($"Failed to start loading scene '{sceneName}'.");
			
			// Preparing
			loadOperation.allowSceneActivation = false;
			NotifyProgress(new(sceneName, 0.0f, SceneTransitionStage.Preparing), progress);

			while (!loadOperation.isDone) {
				// Loading
				float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
				NotifyProgress(new(sceneName, progressValue, SceneTransitionStage.Loading), progress);

				// Activating
				if (loadOperation.progress >= 0.9f) {
					loadOperation.allowSceneActivation = true;
					NotifyProgress(new(sceneName, 1.0f, SceneTransitionStage.Activating), progress);
				}

				await UniTask.Yield();
			}

			// Done
			NotifyProgress(new(sceneName, 1.0f, SceneTransitionStage.Done), progress);
			Debug.Log($"[{nameof(SceneService)}] Loaded scene: {sceneName}");
		}
		private void NotifyProgress(SceneTransitionProgress progress, IProgress<SceneTransitionProgress> observer)
		{
			observer?.Report(progress);

			foreach (ISceneTransitionHook hook in m_Hooks) {
				hook.OnSceneTransitionProgress(progress);
			}
		}
		
		/// Gets the name of the currently active scene.
		public string GetActiveScene() => SceneManager.GetActiveScene().name;

	}
}