using System;
using Gameplay.Enemies.Runtime;
using Gameplay.Flow.GameState;
using Gameplay.Player.Runtime;
using VContainer.Unity;


namespace UI.Gameplay
{
	public sealed class GameplayHudEntryPoint : IStartable, IDisposable
	{
		// === Dependencies ===

		private readonly DiceService           m_PlayerService;
		private readonly EnemyService          m_EnemyService;
		private readonly IGameplayStateService m_GameplayStateService;
		private readonly GameplayHudView       m_View;

		public GameplayHudEntryPoint(DiceService           playerService,        EnemyService    enemyService, 
		                             IGameplayStateService gameplayStateService, GameplayHudView view)
		{
			m_PlayerService        = playerService;
			m_EnemyService         = enemyService;
			m_GameplayStateService = gameplayStateService;
			m_View                 = view;
		}

		// === Lifecycle ===

		public void Start()
		{
			if (m_View != null) {
				m_View.Initialize(m_PlayerService, m_EnemyService, m_GameplayStateService);
			}
		}

		public void Dispose()
		{
			if (m_View != null) {
				m_View.Shutdown();
			}
		}
	}
}
