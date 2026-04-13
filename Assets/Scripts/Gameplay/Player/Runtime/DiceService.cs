using Cysharp.Threading.Tasks;
using Gameplay.Flow.GameState;
using Gameplay.Navigation.Tracing;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Authoring;
using Gameplay.Player.Configuration;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation;
using Gameplay.Player.Presentation.Combat;
using Gameplay.World.Runtime;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;


namespace Gameplay.Player.Runtime
{
	public sealed class DiceService
	{
		// === Dependencies ===
		[Inject] private readonly INavigationService    m_NavigationService;
		[Inject] private readonly LevelNodeService      m_LevelNodeService;
		[Inject] private readonly IGameplayStateService m_GameplayStateService;

		// === Runtime ===
		private DiceBehaviour   m_Player;
		private IDiceView       m_DiceView;
		private DiceController  m_Controller;
		private DiceConfig      m_Config;
		private PlayerGridActor m_GridActor;

		#region Properties

		public bool      HasPlayer     => m_Player != null;
		public DiceState State         => m_Controller?.State ?? default;
		
		public bool      IsAlive       => HasPlayer && CurrentHealth > 0;
		public int       CurrentHealth { get; private set; }
		public int       MaxHealth     => m_Config != null ? m_Config.MaxHealth : 0;

		public Vector2Int Position     => m_Controller != null ? m_Controller.State.Position : default;
		public GameObject PlayerObject => m_Player     != null ? m_Player.gameObject : null;
		public bool       InAction     { get; private set; }

		#endregion

		#region Player Lifecycle

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
			m_Player      = null;
			m_DiceView    = null;
			m_Controller  = null;
			m_Config      = null;
			m_GridActor   = null;
			CurrentHealth = 0;
			InAction      = false;
		}

		#endregion

		#region Combat

		public int ApplyDamage(int damage, GameObject source = null)
		{
			if (!CanTakeDamage(damage)) {
				return 0;
			}

			int consumedDamage = Mathf.Min(CurrentHealth, damage);
			CurrentHealth -= consumedDamage;

			if (CurrentHealth <= 0) {
				HandleDeath();
			}

			return consumedDamage;
		}

		#endregion

		#region Actions

		public async UniTask<bool> TryRollAsync(RollDirection direction)
		{
			if (!CanStartAction()) {
				return false;
			}

			DiceState nextState = m_Controller.PreviewRoll(direction);
			if (!m_NavigationService.CanOccupy(nextState.Position)) {
				return false;
			}

			InAction = true;

			try {
				DiceState currentState = m_Controller.State;
				DiceState appliedState = m_Controller.Roll(direction);

				await m_DiceView.PlayRollAsync(currentState, appliedState, direction, m_NavigationService.Basis);

				LeaveCell(currentState.Position);
				m_NavigationService.TryMoveEntity(m_GridActor, currentState.Position, appliedState.Position);
				EnterCell(appliedState.Position);
				return true;
			} finally {
				InAction = false;
			}
		}

		public async UniTask<bool> TryShootAsync(Vector3 aimPoint)
		{
			if (!CanStartAction()) {
				return false;
			}

			GridBasis basis = m_NavigationService.Basis;
			if (!m_Controller.State.TryResolveShot(basis, aimPoint, out DiceShotDefinition shot)) return false;

			InAction = true;

			try {
				GridTraceResult traceResult = NavGridLineTrace.Trace(m_NavigationService.Grid, m_Controller.State.Position,
				                                                     shot.Direction.ToVector2Int(), m_Config.ShootRange);

				DiceShotPresentationRequest request = new(m_Controller.State.Orientation, shot.Face,
				                                          shot.Direction, shot.ShotCount,
				                                          m_Config.ShootBurstDelay, basis);

				await m_DiceView.PlayShootAsync(request);

				if (traceResult.Entity != null) {
					traceResult.Entity.ApplyDamage(shot.ShotCount, m_Player.gameObject);
				}

				return true;
			} finally {
				InAction = false;
			}
		}

		#endregion

		#region Initialization

		private DiceState InitializePlayerRuntime(DiceBehaviour player, Vector2Int startPosition)
		{
			m_Player   = player;
			m_DiceView = player.View;
			m_Config   = player.Config != null ? player.Config : DiceConfig.CreateRuntimeDefault();
			DiceState state = DiceState.Create(startPosition);
			m_Controller  = new(state);
			m_GridActor   = new(this);
			CurrentHealth = m_Config.MaxHealth;
			
			m_DiceView.Initialize();
			m_DiceView.Snap(state, m_NavigationService.Basis);
			m_NavigationService.TrySetEntity(m_GridActor.Cell, m_GridActor);
			return state;
		}

		#endregion

		#region Validation

		private bool CanTakeDamage(int damage)
		{
			return HasPlayer && damage > 0 && IsAlive;
		}

		private bool CanStartAction()
		{
			return !InAction
			    && m_Controller != null
			    && m_DiceView   != null
			    && m_Config     != null
			    && m_NavigationService.HasLevel
			    && IsAlive;
		}

		#endregion

		#region Cell Notifications

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

		#endregion

		#region Cleanup

		private void HandleDeath()
		{
			LeaveCell(m_Controller.State.Position);
			ClearGridActor();
			m_GameplayStateService.End(GameplayEndReason.PlayerDefeated);
		}

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

		#endregion
	}
}
