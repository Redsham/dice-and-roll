using Cysharp.Threading.Tasks;
using Gameplay.Actors.Runtime;
using Gameplay.Flow.GameState;
using Gameplay.Navigation;
using Gameplay.Nodes.Models;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Authoring;
using Gameplay.Player.Configuration;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation;
using Gameplay.Player.Presentation.Combat;
using Gameplay.World.Runtime;
using Gameplay.World.Runtime.Tracing;
using UnityEngine;


namespace Gameplay.Player.Runtime
{
	public sealed class DiceService : IPlayerService
	{
		// === Dependencies ===

		private readonly INavigationService      m_NavigationService;
		private readonly ILevelNodeService       m_LevelNodeService;
		private readonly IGridLineTraceService   m_GridLineTraceService;
		private readonly ICombatResolverService  m_CombatResolverService;
		private readonly IGameplayStateService   m_GameplayStateService;

		// === Runtime ===

		private DiceBehaviour  m_Player;
		private IDiceView      m_DiceView;
		private DiceController m_Controller;
		private DiceConfig        m_Config;
		private NavLineTraceStep[] m_TraceBuffer;
		private int m_CurrentHealth;
		private PlayerGridActor m_GridActor;

		public DiceService(
			INavigationService navigationService,
			ILevelNodeService levelNodeService,
			IGridLineTraceService gridLineTraceService,
			ICombatResolverService combatResolverService,
			IGameplayStateService gameplayStateService
		)
		{
			m_NavigationService  = navigationService;
			m_LevelNodeService   = levelNodeService;
			m_GridLineTraceService = gridLineTraceService;
			m_CombatResolverService = combatResolverService;
			m_GameplayStateService = gameplayStateService;
		}

		public bool HasPlayer => m_Player != null;
		public DiceState State => m_Controller.State;
		public bool IsAlive => HasPlayer && m_CurrentHealth > 0;
		public int CurrentHealth => m_CurrentHealth;
		public int MaxHealth => m_Config != null ? m_Config.MaxHealth : 0;
		public Vector2Int Position => m_Controller != null ? m_Controller.State.Position : default;
		public GameObject PlayerObject => m_Player != null ? m_Player.gameObject : null;
		public bool IsRolling { get; private set; }

		public void BindPlayer(DiceBehaviour player, Vector2Int startPosition)
		{
			m_Player = player;
			m_DiceView = player.View;
			m_Config = player.Config != null ? player.Config : DiceConfig.CreateRuntimeDefault();
			m_TraceBuffer = new NavLineTraceStep[Mathf.Max(1, m_Config.ShootRange)];

			DiceState initialState = DiceState.Create(startPosition);
			m_Controller = new DiceController(initialState);
			m_CurrentHealth = m_Config.MaxHealth;

			m_DiceView.Initialize();
			m_DiceView.Snap(initialState, m_NavigationService.Basis);
			m_GridActor = new PlayerGridActor(this);
			m_CombatResolverService.RegisterActor(m_GridActor);
			m_LevelNodeService.NotifyActorEntered(initialState.Position, m_Player.gameObject);
		}

		public void ClearPlayer()
		{
			if (m_GridActor != null) {
				if (m_Controller != null && m_Player != null) {
					m_LevelNodeService.NotifyActorLeft(m_Controller.State.Position, m_Player.gameObject);
				}

				m_CombatResolverService.UnregisterActor(m_GridActor);
			}

			if (m_Player != null) {
				Object.Destroy(m_Player.gameObject);
			}

			m_Player = null;
			m_DiceView = null;
			m_Controller = null;
			m_Config = null;
			m_TraceBuffer = null;
			m_GridActor = null;
			m_CurrentHealth = 0;
			IsRolling = false;
		}

		public int ApplyDamage(int damage, GameObject source = null)
		{
			// === Validation ===

			if (!HasPlayer || damage <= 0 || !IsAlive) {
				return 0;
			}

			// === Apply ===

			int consumedDamage = Mathf.Min(m_CurrentHealth, damage);
			m_CurrentHealth -= consumedDamage;

			if (m_CurrentHealth <= 0) {
				m_LevelNodeService.NotifyActorLeft(m_Controller.State.Position, m_Player.gameObject);
				m_CombatResolverService.UnregisterActor(m_GridActor);
				m_GameplayStateService.End(GameplayEndReason.PlayerDefeated);
			}

			return consumedDamage;
		}

		public async UniTask<bool> TryRollAsync(RollDirection direction)
		{
			if (IsRolling || m_Controller == null || !m_NavigationService.HasLevel || !IsAlive) {
				return false;
			}

			DiceState nextState = m_Controller.PreviewRoll(direction);
			if (!m_NavigationService.CanOccupy(nextState.Position)) {
				return false;
			}

			IsRolling = true;

			try {
				DiceState currentState = m_Controller.State;
				DiceState appliedState = m_Controller.Roll(direction);
				await m_DiceView.PlayRollAsync(currentState, appliedState, direction, m_NavigationService.Basis);
				m_LevelNodeService.NotifyActorLeft(currentState.Position, m_Player.gameObject);
				m_CombatResolverService.MoveActor(m_GridActor, currentState.Position, appliedState.Position);
				m_LevelNodeService.NotifyActorEntered(appliedState.Position, m_Player.gameObject);
				return true;
			}
			finally {
				IsRolling = false;
			}
		}

		public async UniTask<bool> TryShootAsync(Vector3 aimPoint)
		{
			if (IsRolling || m_Controller == null || !m_NavigationService.HasLevel || m_DiceView == null || m_Config == null || !IsAlive) {
				return false;
			}

			GridBasis basis = m_NavigationService.Basis;
			Vector3 center = basis.GetCellCenter(m_Controller.State.Position);
			if (!basis.TryGetAimDirection(center, aimPoint, out RollDirection direction)) {
				return false;
			}

			DiceFace face = direction.GetFaceForDirection();
			int shotCount = m_Controller.State.Orientation.GetFaceValue(face);
			if (shotCount <= 0) {
				return false;
			}

			IsRolling = true;

			try {
				System.Array.Clear(m_TraceBuffer, 0, m_TraceBuffer.Length);
				NavLineTraceResult traceResult = m_GridLineTraceService.Trace(
					m_Controller.State.Position,
					direction,
					m_Config.ShootRange,
					shotCount,
					m_TraceBuffer
				);

				DiceShotPresentationRequest request = new(
					face,
					direction,
					shotCount,
					m_Config.ShootBurstDelay,
					basis,
					m_TraceBuffer
				);

				await m_DiceView.PlayShootAsync(request);

				for (int i = 0; i < traceResult.StepCount && i < m_TraceBuffer.Length; i++) {
					NavLineTraceStep step = m_TraceBuffer[i];
					if (step.PowerConsumed > 0) {
						m_CombatResolverService.ApplyDamage(step.Coordinates, step.PowerConsumed, m_Player.gameObject);
					}
				}

				return true;
			}
			finally {
				IsRolling = false;
			}
		}

		// === Actor Adapter ===

		private sealed class PlayerGridActor : IGridActor
		{
			private readonly DiceService m_Owner;

			public PlayerGridActor(DiceService owner)
			{
				m_Owner = owner;
			}

			public GameObject Owner => m_Owner.PlayerObject;
			public Vector2Int Cell => m_Owner.Position;
			public NavCellOccupancy Occupancy => new() { Type = NavCellOccupancyType.Actor };
			public bool IsAlive => m_Owner.IsAlive;

			public NodeProjectileImpactInfo PreviewProjectileImpact(int incomingDamage)
			{
				int consumedDamage = Mathf.Clamp(incomingDamage, 0, m_Owner.CurrentHealth);
				return new NodeProjectileImpactInfo(consumedDamage, consumedDamage > 0, consumedDamage > 0);
			}

			public int ApplyDamage(int damage, GameObject source = null)
			{
				return m_Owner.ApplyDamage(damage, source);
			}
		}
	}
}
