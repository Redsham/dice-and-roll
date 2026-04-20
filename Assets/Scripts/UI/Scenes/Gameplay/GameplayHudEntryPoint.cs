using System;
using Gameplay.Composition;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace UI.Scenes.Gameplay
{
	public sealed class GameplayHudEntryPoint : IStartable, IDisposable
	{
		// === Dependencies ===

		[Inject] private readonly IObjectResolver            m_Resolver;
		[Inject] private readonly GameplaySceneConfiguration m_Configuration;

		// === State ===

		private GameplayHudView m_View;

		// === Lifecycle ===

		public void Start()
		{
			if (m_Configuration == null) {
				Debug.LogError($"[{nameof(GameplayHudEntryPoint)}] Missing dependency: {nameof(GameplaySceneConfiguration)}. Cannot initialize Gameplay HUD.");
				return;
			}

			if (m_Configuration.HudPrefab == null) {
				Debug.LogError($"[{nameof(GameplayHudEntryPoint)}] Missing HUD prefab in configuration. Cannot initialize Gameplay HUD.");
				return;
			}

			m_View = m_Resolver.Instantiate(m_Configuration.HudPrefab, parent: null);
		}
		public void Dispose() => m_View.Shutdown();
	}
}