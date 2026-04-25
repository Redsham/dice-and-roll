using Gameplay.Flow.GameState;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Authoring;
using Gameplay.Player.Configuration;
using Gameplay.Player.Domain;
using Gameplay.Player.Presentation;
using Gameplay.World.Runtime;
using R3;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;


namespace Gameplay.Player.Runtime
{
	public partial class DiceService
	{
		#region Properties
		
		// === Dependencies ===

		[Inject] private readonly INavigationService    m_NavigationService;
		[Inject] private readonly LevelNodeService      m_LevelNodeService;
		[Inject] private readonly IGameplayStateService m_GameplayStateService;

		// === Runtime ===

		private DiceBehaviour  m_Player;
		private DiceView       m_DiceView;
		private DiceController m_Controller;
		private DiceConfig     m_Config;
		private DiceGridActor  m_GridActor;
		
		public bool      HasPlayer => m_Player != null;
		public DiceState State     => m_Controller?.State ?? default;

		public bool                  IsAlive       => HasPlayer && CurrentHealth.Value > 0;
		public ReactiveProperty<int> CurrentHealth { get; private set; } = new(0);
		public int                   MaxHealth     => m_Config != null ? m_Config.MaxHealth : 0;

		public Vector2Int Position     => m_Controller != null ? m_Controller.State.Position : default;
		public GameObject PlayerObject => m_Player     != null ? m_Player.gameObject : null;
		public bool       InAction     { get; private set; }

		#endregion

		// === Lifecycle ===

		public void BindPlayer(DiceBehaviour player, Vector2Int startPosition)
		{
			DiceState initialState = InitializePlayerRuntime(player, startPosition);
			EnterCell(initialState.Position);
		}
		public void ClearPlayer()
		{
			if (HasPlayer && m_Controller != null) {
				LeaveCell(m_Controller.State.Position);
			}

			ClearGridActor();
			DestroyPlayerObject();

			// The service is long-lived within the gameplay scope, so it must forget the
			// destroyed player instance before the next level/player bind happens.
			m_Player            = null;
			m_DiceView          = null;
			m_Controller        = null;
			m_Config            = null;
			m_GridActor         = null;
			CurrentHealth.Value = 0;
			InAction            = false;
		}

		private DiceState InitializePlayerRuntime(DiceBehaviour player, Vector2Int startPosition)
		{
			m_Player   = player;
			m_DiceView = player.View;
			m_Config   = player.Config != null ? player.Config : DiceConfig.CreateRuntimeDefault();
			DiceState state = DiceState.Create(startPosition);
			m_Controller        = new(state);
			m_GridActor         = new(this);
			CurrentHealth.Value = m_Config.MaxHealth;

			m_DiceView.Initialize(m_Config.ShootRange);
			m_DiceView.Snap(state, m_NavigationService.Basis);
			m_NavigationService.TrySetEntity(m_GridActor.Cell, m_GridActor);
			return state;
		}
		private void Died()
		{
			LeaveCell(m_Controller.State.Position);
			ClearGridActor();
			m_GameplayStateService.End(GameplayEndReason.PlayerDefeated);
		}
		
		// === Combat ===
		
		public int ApplyDamage(int damage, GameObject source = null)
		{
			if (!HasPlayer && damage > 0 && IsAlive) return 0;

			int consumedDamage = Mathf.Min(CurrentHealth.Value, damage);
			CurrentHealth.Value -= consumedDamage;

			if (CurrentHealth.Value <= 0) {
				Died();
			}

			return consumedDamage;
		}

		// === Cell Notifications ===
		
		private void EnterCell(Vector2Int cell)
		{
			if (m_Player != null) {
				m_LevelNodeService.NotifyActorEntered(cell, m_Player.gameObject);
			}
		}
		private void LeaveCell(Vector2Int cell)
		{
			if (m_Player != null) {
				m_LevelNodeService.NotifyActorLeft(cell, m_Player.gameObject);
			}
		}

		// === Cleanup ===

		private void ClearGridActor()
		{
			if (m_GridActor != null) {
				m_NavigationService.TryClearEntity(m_GridActor.Cell, m_GridActor);
			}
		}
		private void DestroyPlayerObject()
		{
			if (m_Player != null) {
				Object.Destroy(m_Player.gameObject);
			}
		}
	}
}