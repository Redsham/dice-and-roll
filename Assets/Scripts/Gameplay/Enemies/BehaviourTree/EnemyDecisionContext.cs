using Gameplay.Enemies.Runtime;
using Gameplay.Navigation.Pathfinding;
using Gameplay.Player.Domain;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Enemies.BehaviourTree
{
	public sealed class EnemyDecisionContext
	{
		public EnemyRuntimeHandle Enemy             { get; }
		public IPlayerService     PlayerService     { get; }
		public INavigationService NavigationService { get; }

		public EnemyTurnAction SelectedAction    { get; private set; }
		public bool            HasSelectedAction => SelectedAction.Type != EnemyTurnActionType.None;

		public EnemyDecisionContext(
			EnemyRuntimeHandle enemy,
			IPlayerService     playerService,
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

		public bool TryGetPawnPathDirectionToPlayer(out RollDirection direction)
		{
			PawnTurnPriorityTraversalCostProvider weights    = default;
			Vector2Int[]                         pathBuffer = new Vector2Int[8];
			NavPathResult                        result;

			while (!NavigationService.TryFindPath(Enemy.State.Position, PlayerService.Position, ref weights, pathBuffer, out result)) {
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

			Vector2Int step = pathBuffer[1] - pathBuffer[0];
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
