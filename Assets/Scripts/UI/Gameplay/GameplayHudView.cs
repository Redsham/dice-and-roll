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

		private DiceService           m_PlayerService;
		private EnemyService          m_EnemyService;
		private IGameplayStateService m_GameplayStateService;
		private SceneService          m_SceneService;
		private UIFade                m_Fade;

		// === API ===

		public void Initialize(DiceService playerService, EnemyService enemyService, IGameplayStateService gameplayStateService, SceneService sceneService, UIFade fade)
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
