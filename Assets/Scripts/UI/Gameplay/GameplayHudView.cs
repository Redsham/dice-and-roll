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
		[SerializeField] private PlayerHealth m_PlayerHealth;

		// === Dependencies ===

		private DiceService           m_PlayerService;
		private SceneService          m_SceneService;
		private UIFade                m_Fade;

		// === API ===

		public void Initialize(DiceService playerService, EnemyService enemyService, IGameplayStateService gameplayStateService)
		{
			m_PlayerService        = playerService;
			
			if (m_PlayerService != null) {
				m_PlayerService.HealthChanged += OnHealthChanged;
				OnHealthChanged(m_PlayerService.CurrentHealth, m_PlayerService.MaxHealth);
			}
		}

		public void Shutdown()
		{
			if (m_PlayerService != null) {
				m_PlayerService.HealthChanged -= OnHealthChanged;
			}
		}

		private void OnHealthChanged(int currentHealth, int maxHealth)
		{
			if (m_PlayerHealth != null) {
				m_PlayerHealth.SetHealth(currentHealth, maxHealth);
			}
		}
	}
}
