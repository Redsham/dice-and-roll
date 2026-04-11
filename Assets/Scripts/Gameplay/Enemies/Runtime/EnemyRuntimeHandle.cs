using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Actors.Runtime;
using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.BehaviourTree;
using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Presentation;
using Gameplay.Navigation;
using Gameplay.Nodes.Models;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Domain;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using Gameplay.World.Runtime.Tracing;
using UnityEngine;


namespace Gameplay.Enemies.Runtime
{
	public sealed class EnemyRuntimeHandle : IGridActor
	{
		// === Dependencies ===

		private readonly IPlayerService          m_PlayerService;
		private readonly INavigationService      m_NavigationService;
		private readonly ICombatResolverService  m_CombatResolverService;
		private readonly IGridLineTraceService   m_GridLineTraceService;
		private readonly ILevelNodeService       m_LevelNodeService;

		// === Authoring ===

		private readonly EnemyBehaviour  m_Behaviour;
		private readonly EnemyView       m_View;
		private readonly EnemyConfig     m_Config;
		private readonly BehaviourTreeRunner m_BehaviourTree;

		// === Runtime ===

		private readonly NavLineTraceStep[] m_TraceBuffer;
		private EnemyState m_State;
		private Vector2Int? m_PendingMortarCell;
		private int m_MortarTurnsUntilImpact;

		public EnemyRuntimeHandle(
			EnemyBehaviour enemyBehaviour,
			IPlayerService playerService,
			INavigationService navigationService,
			ICombatResolverService combatResolverService,
			IGridLineTraceService gridLineTraceService,
			ILevelNodeService levelNodeService
		)
		{
			// === Validation ===

			if (enemyBehaviour == null) {
				throw new System.ArgumentNullException(nameof(enemyBehaviour));
			}

			if (enemyBehaviour.View == null) {
				throw new System.InvalidOperationException(
					$"Enemy prefab '{enemyBehaviour.name}' must reference an {nameof(EnemyView)} on {nameof(EnemyBehaviour)}."
				);
			}

			if (enemyBehaviour.Config == null) {
				throw new System.InvalidOperationException(
					$"Enemy prefab '{enemyBehaviour.name}' must reference an {nameof(EnemyConfig)} on {nameof(EnemyBehaviour)}."
				);
			}

			m_Behaviour = enemyBehaviour;
			m_View = enemyBehaviour.View;
			m_Config = enemyBehaviour.Config;
			m_PlayerService = playerService;
			m_NavigationService = navigationService;
			m_CombatResolverService = combatResolverService;
			m_GridLineTraceService = gridLineTraceService;
			m_LevelNodeService = levelNodeService;
			m_State = EnemyState.Create(enemyBehaviour.GridPosition, m_Config.MaxHealth);
			m_State.Facing = enemyBehaviour.InitialFacing;
			m_TraceBuffer = new NavLineTraceStep[Mathf.Max(1, (enemyBehaviour as PawnEnemyBehaviour)?.Config.ShootRange ?? 1)];
			m_BehaviourTree = EnemyBehaviourTreeFactory.Create(this);
			enemyBehaviour.BindRuntime(this);
		}

		// === Identity ===

		public EnemyKind Kind => m_Behaviour.Kind;
		public EnemyBehaviour Behaviour => m_Behaviour;
		public EnemyConfig Config => m_Config;
		public BehaviourTreeDebugView DebugView => m_BehaviourTree.DebugView;

		// === Actor ===

		public GameObject Owner => m_Behaviour.gameObject;
		public Vector2Int Cell => m_State.Position;
		public NavCellOccupancy Occupancy => m_Behaviour.CreateOccupancy();
		public bool IsAlive => m_State.CurrentHealth > 0;

		// === State ===

		public EnemyState State => m_State;
		public Vector2Int? PendingMortarCell => m_PendingMortarCell;
		public int MortarTurnsUntilImpact => m_MortarTurnsUntilImpact;

		// === Lifecycle ===

		public async UniTask SpawnAsync(CancellationToken cancellationToken)
		{
			if (m_Behaviour is MortarEnemyBehaviour mortar && mortar.AimMarker != null) {
				mortar.AimMarker.Hide();
			}

			await m_View.PlaySpawnAsync(m_State.Position, m_State.Facing, m_NavigationService.Basis, m_Config.SpawnDuration, cancellationToken);
			m_LevelNodeService.NotifyActorEntered(m_State.Position, m_Behaviour.gameObject);
		}

		public async UniTask ExecuteTurnAsync(CancellationToken cancellationToken)
		{
			if (!IsAlive || !m_PlayerService.IsAlive) {
				return;
			}

			EnemyDecisionContext context = new(this, m_PlayerService, m_NavigationService);
			EnemyTurnAction action = m_BehaviourTree.Evaluate(context);
			await ExecuteActionAsync(action, cancellationToken);
		}

		// === Combat ===

		public NodeProjectileImpactInfo PreviewProjectileImpact(int incomingDamage)
		{
			int consumedDamage = Mathf.Clamp(incomingDamage, 0, m_State.CurrentHealth);
			return new NodeProjectileImpactInfo(consumedDamage, consumedDamage > 0, consumedDamage > 0);
		}

		public int ApplyDamage(int damage, GameObject source = null)
		{
			if (!IsAlive || damage <= 0) {
				return 0;
			}

			int consumedDamage = Mathf.Min(damage, m_State.CurrentHealth);
			m_State.CurrentHealth -= consumedDamage;

			if (!IsAlive) {
				m_LevelNodeService.NotifyActorLeft(m_State.Position, m_Behaviour.gameObject);
				if (m_Behaviour is MortarEnemyBehaviour mortar && mortar.AimMarker != null) {
					mortar.AimMarker.Hide();
				}

				Object.Destroy(m_Behaviour.gameObject);
			}

			return consumedDamage;
		}

		// === Mortar State ===

		public void ScheduleMortarStrike(Vector2Int cell, int turnsUntilImpact)
		{
			m_PendingMortarCell = cell;
			m_MortarTurnsUntilImpact = turnsUntilImpact;
			if (m_Behaviour is MortarEnemyBehaviour mortar && mortar.AimMarker != null) {
				mortar.AimMarker.Show(cell, m_NavigationService.Basis);
			}
		}

		// === Actions ===

		private async UniTask ExecuteActionAsync(EnemyTurnAction action, CancellationToken cancellationToken)
		{
			switch (action.Type) {
				case EnemyTurnActionType.Move:
					await ExecuteMoveAsync(action.Direction, cancellationToken);
					break;
				case EnemyTurnActionType.Rotate:
					await ExecuteRotateAsync(action.Direction, cancellationToken);
					break;
				case EnemyTurnActionType.Shoot:
					await ExecuteShootAsync(action.TargetCell, cancellationToken);
					break;
				case EnemyTurnActionType.Wait:
				case EnemyTurnActionType.None:
				default:
					break;
			}

			AdvanceMortarCountdown();
		}

		private async UniTask ExecuteMoveAsync(RollDirection direction, CancellationToken cancellationToken)
		{
			Vector2Int nextCell = m_State.Position.Move(direction);
			if (!m_NavigationService.CanOccupy(nextCell)) {
				return;
			}

			Vector2Int previousCell = m_State.Position;
			m_State.Position = nextCell;
			m_State.Facing = direction;

			await m_View.PlayMoveAsync(previousCell, nextCell, m_NavigationService.Basis, m_Config.MoveDuration, cancellationToken);
			m_LevelNodeService.NotifyActorLeft(previousCell, m_Behaviour.gameObject);
			m_CombatResolverService.MoveActor(this, previousCell, nextCell);
			m_LevelNodeService.NotifyActorEntered(nextCell, m_Behaviour.gameObject);
		}

		private async UniTask ExecuteRotateAsync(RollDirection direction, CancellationToken cancellationToken)
		{
			m_State.Facing = direction;
			await m_View.PlayRotateAsync(direction, m_NavigationService.Basis, m_Config.RotateDuration, cancellationToken);
		}

		private async UniTask ExecuteShootAsync(Vector2Int targetCell, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await UniTask.Yield();

			switch (Kind) {
				case EnemyKind.Pawn:
					await ExecutePawnShootAsync();
					break;
				case EnemyKind.Mortar:
					await ExecuteMortarShootAsync(targetCell);
					break;
			}
		}

		private async UniTask ExecutePawnShootAsync()
		{
			if (m_Behaviour is not PawnEnemyBehaviour pawn) {
				return;
			}

			NavLineTraceResult traceResult = m_GridLineTraceService.Trace(
				m_State.Position,
				m_State.Facing,
				pawn.Config.ShootRange,
				pawn.Config.ShootDamage,
				m_TraceBuffer
			);

			for (int i = 0; i < traceResult.StepCount && i < m_TraceBuffer.Length; i++) {
				NavLineTraceStep step = m_TraceBuffer[i];
				if (step.PowerConsumed > 0) {
					m_CombatResolverService.ApplyDamage(step.Coordinates, step.PowerConsumed, m_Behaviour.gameObject);
				}
			}

			await UniTask.CompletedTask;
		}

		private async UniTask ExecuteMortarShootAsync(Vector2Int targetCell)
		{
			if (m_Behaviour is not MortarEnemyBehaviour mortar) {
				return;
			}

			if (m_PendingMortarCell.HasValue && m_PendingMortarCell.Value == targetCell) {
				m_CombatResolverService.ApplyDamage(targetCell, mortar.Config.BombardmentDamage, m_Behaviour.gameObject);
			}

			m_PendingMortarCell = null;
			m_MortarTurnsUntilImpact = 0;
			if (mortar.AimMarker != null) {
				mortar.AimMarker.Hide();
			}

			await UniTask.CompletedTask;
		}

		private void AdvanceMortarCountdown()
		{
			if (!m_PendingMortarCell.HasValue || m_MortarTurnsUntilImpact <= 0) {
				return;
			}

			m_MortarTurnsUntilImpact--;
		}
	}
}
