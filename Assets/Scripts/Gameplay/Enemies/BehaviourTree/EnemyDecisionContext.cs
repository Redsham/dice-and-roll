using Gameplay.Enemies.Runtime;
using Gameplay.Navigation.Tracing;
using Gameplay.Navigation.Pathfinding;
using Gameplay.Player.Domain;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Enemies.BehaviourTree
{
	public sealed class EnemyDecisionContext
	{
		private const int InitialPathBufferSize = 8;

		public EnemyRuntimeHandle Enemy             { get; }
		public DiceService        PlayerService     { get; }
		public INavigationService NavigationService { get; }

		public EnemyTurnAction SelectedAction    { get; private set; }
		public bool            HasSelectedAction => SelectedAction.Type != EnemyTurnActionType.None;

		public EnemyDecisionContext(
			EnemyRuntimeHandle enemy,
			DiceService        playerService,
			INavigationService navigationService
		)
		{
			Enemy             = enemy;
			PlayerService     = playerService;
			NavigationService = navigationService;
			SelectedAction    = EnemyTurnAction.None();
		}

		public void SelectAction(EnemyTurnAction action)
		{
			SelectedAction = action;
		}

		public void ResetAction()
		{
			SelectedAction = EnemyTurnAction.None();
		}

		public Vector2Int GetDeltaToPlayer()
		{
			return PlayerService.Position - Enemy.State.Position;
		}

		public bool TryGetPrimaryDirectionToPlayer(out RollDirection direction)
		{
			Vector2Int delta = GetDeltaToPlayer();
			if (delta == Vector2Int.zero) {
				direction = default;
				return false;
			}

			if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)) {
				direction = delta.x >= 0 ? RollDirection.East : RollDirection.West;
				return true;
			}

			direction = delta.y >= 0 ? RollDirection.North : RollDirection.South;
			return true;
		}

		public bool TryGetDirectApproachDirection(out RollDirection direction)
		{
			Vector2Int delta = GetDeltaToPlayer();
			if (delta == Vector2Int.zero) {
				direction = default;
				return false;
			}

			if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)) {
				if (TryGetApproachDirection(delta.x >= 0 ? RollDirection.East : RollDirection.West, out direction)) {
					return true;
				}

				return TryGetApproachDirection(delta.y >= 0 ? RollDirection.North : RollDirection.South, out direction);
			}

			if (TryGetApproachDirection(delta.y >= 0 ? RollDirection.North : RollDirection.South, out direction)) {
				return true;
			}

			return TryGetApproachDirection(delta.x >= 0 ? RollDirection.East : RollDirection.West, out direction);
		}

		public bool TryGetPawnPathDirectionToPlayer(out RollDirection direction)
		{
			return TryGetPathDirection(Enemy.State.Position, PlayerService.Position, out direction);
		}

		public bool TryGetPawnAdvanceDirection(int shootRange, out RollDirection direction)
		{
			PawnTurnPriorityTraversalCostProvider weights        = CreatePawnWeights();
			Vector2Int                           bestTargetCell = default;
			int                                  bestTotalCost  = int.MaxValue;
			int                                  bestPathLength = int.MaxValue;
			bool                                 hasBestCell    = false;

			for (int distance = 1; distance <= shootRange; distance++) {
				if (EvaluatePawnAttackCell(PlayerService.Position + Vector2Int.up * distance, shootRange, ref weights, ref bestTargetCell, ref bestTotalCost, ref bestPathLength)) {
					hasBestCell = true;
				}

				if (EvaluatePawnAttackCell(PlayerService.Position + Vector2Int.right * distance, shootRange, ref weights, ref bestTargetCell, ref bestTotalCost, ref bestPathLength)) {
					hasBestCell = true;
				}

				if (EvaluatePawnAttackCell(PlayerService.Position + Vector2Int.down * distance, shootRange, ref weights, ref bestTargetCell, ref bestTotalCost, ref bestPathLength)) {
					hasBestCell = true;
				}

				if (EvaluatePawnAttackCell(PlayerService.Position + Vector2Int.left * distance, shootRange, ref weights, ref bestTargetCell, ref bestTotalCost, ref bestPathLength)) {
					hasBestCell = true;
				}
			}

			if (hasBestCell) {
				return TryGetPathDirection(Enemy.State.Position, bestTargetCell, out direction);
			}

			direction = default;
			return false;
		}

		public bool CanShootPlayerFrom(Vector2Int originCell, int shootRange)
		{
			Vector2Int delta = PlayerService.Position - originCell;
			if (delta == Vector2Int.zero || (delta.x != 0 && delta.y != 0)) {
				return false;
			}

			int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
			if (distance <= 0 || distance > shootRange) {
				return false;
			}

			Vector2Int direction = delta.x != 0
				? new(Mathf.RoundToInt(Mathf.Sign(delta.x)), 0)
				: new(0, Mathf.RoundToInt(Mathf.Sign(delta.y)));

			GridTraceResult traceResult = NavGridLineTrace.Trace(NavigationService.Grid, originCell, direction, distance);
			return traceResult.Hit && traceResult.Point == PlayerService.Position;
		}

		private bool EvaluatePawnAttackCell(
			Vector2Int                              candidateCell,
			int                                     shootRange,
			ref PawnTurnPriorityTraversalCostProvider weights,
			ref Vector2Int                          bestTargetCell,
			ref int                                 bestTotalCost,
			ref int                                 bestPathLength
		)
		{
			if (candidateCell == Enemy.State.Position
			    || !NavigationService.CanOccupy(candidateCell)
			    || !CanShootPlayerFrom(candidateCell, shootRange)) {
				return false;
			}

			Vector2Int[] pathBuffer = new Vector2Int[InitialPathBufferSize];
			NavPathResult result;
			while (!NavigationService.TryFindPath(Enemy.State.Position, candidateCell, ref weights, pathBuffer, out result)) {
				if (result.Status != NavPathStatus.BufferTooSmall || result.RequiredSize <= pathBuffer.Length) {
					return false;
				}

				pathBuffer = new Vector2Int[result.RequiredSize];
			}

			if (result.PathLength < 2) {
				return false;
			}

			if (result.TotalCost > bestTotalCost) {
				return false;
			}

			if (result.TotalCost == bestTotalCost && result.PathLength >= bestPathLength) {
				return false;
			}

			bestTargetCell = candidateCell;
			bestTotalCost  = result.TotalCost;
			bestPathLength = result.PathLength;
			return true;
		}

		private bool TryGetPathDirection(Vector2Int start, Vector2Int goal, out RollDirection direction)
		{
			PawnTurnPriorityTraversalCostProvider weights    = CreatePawnWeights();
			Vector2Int[]                         pathBuffer = new Vector2Int[InitialPathBufferSize];
			NavPathResult                        result;

			while (!NavigationService.TryFindPath(start, goal, ref weights, pathBuffer, out result)) {
				if (result.Status != NavPathStatus.BufferTooSmall || result.RequiredSize <= pathBuffer.Length) {
					direction = default;
					return false;
				}

				pathBuffer = new Vector2Int[result.RequiredSize];
			}

			if (result.PathLength < 2) {
				direction = default;
				return false;
			}

			return TryGetDirectionFromStep(pathBuffer[1] - pathBuffer[0], out direction);
		}

		private PawnTurnPriorityTraversalCostProvider CreatePawnWeights()
		{
			return new() {
				InitialFacing = Enemy.State.Facing
			};
		}

		private bool TryGetApproachDirection(RollDirection candidate, out RollDirection direction)
		{
			Vector2Int nextCell = Enemy.State.Position.Move(candidate);
			if (NavigationService.CanOccupy(nextCell)) {
				direction = candidate;
				return true;
			}

			direction = default;
			return false;
		}

		private static bool TryGetDirectionFromStep(Vector2Int step, out RollDirection direction)
		{
			if (step == Vector2Int.up) {
				direction = RollDirection.North;
				return true;
			}

			if (step == Vector2Int.right) {
				direction = RollDirection.East;
				return true;
			}

			if (step == Vector2Int.down) {
				direction = RollDirection.South;
				return true;
			}

			if (step == Vector2Int.left) {
				direction = RollDirection.West;
				return true;
			}

			direction = default;
			return false;
		}
	}
}
