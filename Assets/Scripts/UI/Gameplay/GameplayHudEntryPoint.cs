using System;
using Gameplay.Enemies.Runtime;
using Gameplay.Flow.GameState;
using Gameplay.Player.Runtime;
using Infrastructure.Services.Scenes;
using UI.Effects;


namespace UI.Gameplay
{
	public sealed class GameplayHudEntryPoint : IDisposable
	{
		// === Dependencies ===

		private readonly IPlayerService        m_PlayerService;
		private readonly IEnemyService         m_EnemyService;
		private readonly IGameplayStateService m_GameplayStateService;
		private readonly SceneService          m_SceneService;
		private readonly UIFade                m_Fade;

		// === Runtime ===

		private GameplayHudView m_View;

		public GameplayHudEntryPoint(IPlayerService playerService, IEnemyService enemyService, IGameplayStateService gameplayStateService, SceneService sceneService, UIFade fade
		)
		{
			m_PlayerService        = playerService;
			m_EnemyService         = enemyService;
			m_GameplayStateService = gameplayStateService;
			m_SceneService         = sceneService;
			m_Fade                 = fade;
		}

		// === Lifecycle ===

		public void Dispose()
		{
			if (m_View != null) {
				m_View.Shutdown();
			}
		}
	}
}