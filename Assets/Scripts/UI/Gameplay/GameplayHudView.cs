using Gameplay.Enemies.Runtime;
using Gameplay.Flow.GameState;
using Gameplay.Player.Runtime;
using Infrastructure.Services.Scenes;
using UI.Effects;
using UnityEngine;


namespace UI.Gameplay
{
	public sealed class GameplayHudView : MonoBehaviour
	{
		// === Dependencies ===

		private IPlayerService        m_PlayerService;
		private IEnemyService         m_EnemyService;
		private IGameplayStateService m_GameplayStateService;
		private SceneService          m_SceneService;
		private UIFade                m_Fade;

		// === API ===

		public void Initialize(IPlayerService playerService, IEnemyService enemyService, IGameplayStateService gameplayStateService, SceneService sceneService, UIFade fade)
		{
			m_PlayerService        = playerService;
			m_EnemyService         = enemyService;
			m_GameplayStateService = gameplayStateService;
			m_SceneService         = sceneService;
			m_Fade                 = fade;
		}

		public void Shutdown()
		{
			
		}
	}
}