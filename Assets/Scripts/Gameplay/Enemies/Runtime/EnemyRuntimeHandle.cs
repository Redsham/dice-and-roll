using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.BehaviourTree;
using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Presentation;
using Gameplay.Navigation;
using Gameplay.Navigation.Tracing;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Domain;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Gameplay.Enemies.Runtime
{
	public sealed class EnemyRuntimeHandle : INavCellEntity
	{
		// === Dependencies ===

		private readonly DiceService            m_PlayerService;
		private readonly INavigationService     m_NavigationService;
		private readonly LevelNodeService       m_LevelNodeService;

		// === Authoring ===

		private readonly EnemyView           m_View;
		private readonly BehaviourTreeRunner m_BehaviourTree;

		// === Runtime ===

		private          EnemyState         m_State;
		private          Vector2Int?        m_PendingMortarCell;

		public EnemyRuntimeHandle(
			EnemyBehaviour         enemyBehaviour,
			DiceService            playerService,
			INavigationService     navigationService,
			LevelNodeService       levelNodeService
		)
		{
			// === Validation ===

			if (enemyBehaviour == null) {
				throw new ArgumentNullException(nameof(enemyBehaviour));
			}

			if (enemyBehaviour.View == null) {
				throw new InvalidOperationException(
				                                    $"Enemy prefab '{enemyBehaviour.name}' must reference an {nameof(EnemyView)} on {nameof(EnemyBehaviour)}."
				                                   );
			}

			if (enemyBehaviour.Config == null) {
				throw new InvalidOperationException(
				                                    $"Enemy prefab '{enemyBehaviour.name}' must reference an {nameof(EnemyConfig)} on {nameof(EnemyBehaviour)}."
				                                   );
			}

			Behaviour               = enemyBehaviour;
			m_View                  = enemyBehaviour.View;
			Config                  = enemyBehaviour.Config;
			m_PlayerService         = playerService;
			m_NavigationService     = navigationService;
			m_LevelNodeService      = levelNodeService;
			m_State                 = EnemyState.Create(enemyBehaviour.GridPosition, Config.MaxHealth);
			m_State.Facing          = enemyBehaviour.InitialFacing;
			m_BehaviourTree         = EnemyBehaviourTreeFactory.Create(this);
			Behaviour.SetGridPosition(m_State.Position);
			enemyBehaviour.BindRuntime(this);
		}

		// === Identity ===

		public EnemyKind Kind => Behaviour.Kind;
		public EnemyBehaviour Behaviour
		{
			get;
		}
		public EnemyConfig Config
		{
			get;
		}
		public BehaviourTreeDebugView DebugView => m_BehaviourTree.DebugView;

		// === Actor ===

		public NavCellEntityLayer Layer => NavCellEntityLayer.Actor;
		public GameObject   Owner   => Behaviour.gameObject;
		public Vector2Int   Cell    => m_State.Position;
		public NavCellFlags Flags   => NavCellFlags.None;
		public bool         IsAlive => m_State.CurrentHealth > 0;

		// === State ===

		public EnemyState  State             => m_State;
		public Vector2Int? PendingMortarCell => m_PendingMortarCell;
		public int MortarTurnsUntilImpact
		{
			get;
			private set;
		}

		// === Lifecycle ===

		public async UniTask SpawnAsync(bool playSpawnAnimation, CancellationToken cancellationToken)
		{
			if (Behaviour is MortarEnemyBehaviour mortar && mortar.AimMarker != null) {
				mortar.AimMarker.Hide();
			}

			if (playSpawnAnimation) {
				await m_View.PlaySpawnAsync(m_State.Position, m_State.Facing, m_NavigationService.Basis, Config.SpawnDuration, cancellationToken);
			}
			else {
				m_View.Snap(m_State.Position, m_State.Facing, m_NavigationService.Basis);
			}

			m_LevelNodeService.NotifyActorEntered(m_State.Position, Behaviour.gameObject);
		}

		public async UniTask ExecuteTurnAsync(CancellationToken cancellationToken)
		{
			if (!IsAlive || !m_PlayerService.IsAlive) {
				return;
			}

			EnemyDecisionContext context = new(this, m_PlayerService, m_NavigationService);
			EnemyTurnAction      action  = m_BehaviourTree.Evaluate(context);
			await ExecuteActionAsync(action, cancellationToken);
		}

		// === Combat ===

		public int ApplyDamage(int damage, GameObject source = null)
		{
			if (!IsAlive || damage <= 0) {
				return 0;
			}

			int consumedDamage = Mathf.Min(damage, m_State.CurrentHealth);
			m_State.CurrentHealth -= consumedDamage;

			if (!IsAlive) {
				m_NavigationService.TryClearEntity(m_State.Position, this);
				m_LevelNodeService.NotifyActorLeft(m_State.Position, Behaviour.gameObject);
				if (Behaviour is MortarEnemyBehaviour mortar && mortar.AimMarker != null) {
					mortar.AimMarker.Hide();
				}

				Object.Destroy(Behaviour.gameObject);
			}

			return consumedDamage;
		}

		// === Mortar State ===

		public void ScheduleMortarStrike(Vector2Int cell, int turnsUntilImpact)
		{
			m_PendingMortarCell    = cell;
			MortarTurnsUntilImpact = turnsUntilImpact;
			if (Behaviour is MortarEnemyBehaviour mortar && mortar.AimMarker != null) {
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
			m_State.Facing   = direction;
			Behaviour.SetGridPosition(nextCell);

			await m_View.PlayMoveAsync(previousCell, nextCell, m_NavigationService.Basis, Config.MoveDuration, cancellationToken);
			m_LevelNodeService.NotifyActorLeft(previousCell, Behaviour.gameObject);
			m_NavigationService.TryMoveEntity(this, previousCell, nextCell);
			m_LevelNodeService.NotifyActorEntered(nextCell, Behaviour.gameObject);
		}

		private async UniTask ExecuteRotateAsync(RollDirection direction, CancellationToken cancellationToken)
		{
			m_State.Facing = direction;
			await m_View.PlayRotateAsync(direction, m_NavigationService.Basis, Config.RotateDuration, cancellationToken);
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
			if (Behaviour is not PawnEnemyBehaviour pawn) {
				return;
			}

			GridTraceResult traceResult = NavGridLineTrace.Trace(
			                                                    m_NavigationService.Grid,
			                                                    m_State.Position,
			                                                    m_State.Facing.ToVector2Int(),
			                                                    pawn.Config.ShootRange
			                                                   );

			if (traceResult.Entity != null) {
				traceResult.Entity.ApplyDamage(pawn.Config.ShootDamage, Behaviour.gameObject);
			}

			await UniTask.CompletedTask;
		}

		private async UniTask ExecuteMortarShootAsync(Vector2Int targetCell)
		{
			if (Behaviour is not MortarEnemyBehaviour mortar) {
				return;
			}

			if (m_PendingMortarCell.HasValue && m_PendingMortarCell.Value == targetCell) {
				if (m_NavigationService.TryGetEntity(targetCell, out INavCellEntity entity) && entity != null) {
					entity.ApplyDamage(mortar.Config.BombardmentDamage, Behaviour.gameObject);
				}
			}

			m_PendingMortarCell    = null;
			MortarTurnsUntilImpact = 0;
			if (mortar.AimMarker != null) {
				mortar.AimMarker.Hide();
			}

			await UniTask.CompletedTask;
		}

		private void AdvanceMortarCountdown()
		{
			if (!m_PendingMortarCell.HasValue || MortarTurnsUntilImpact <= 0) {
				return;
			}

			MortarTurnsUntilImpact--;
		}
	}
}
