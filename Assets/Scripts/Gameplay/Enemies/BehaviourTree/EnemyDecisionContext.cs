using Gameplay.Player.Domain;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Enemies.BehaviourTree
{
	public sealed class EnemyDecisionContext
	{
		public Runtime.EnemyRuntimeHandle Enemy { get; }
		public IPlayerService PlayerService { get; }
		public INavigationService NavigationService { get; }

		public Runtime.EnemyTurnAction SelectedAction { get; private set; }
		public bool HasSelectedAction => SelectedAction.Type != Runtime.EnemyTurnActionType.None;

		public EnemyDecisionContext(
			Runtime.EnemyRuntimeHandle enemy,
			IPlayerService playerService,
			INavigationService navigationService
		)
		{
			Enemy = enemy;
			PlayerService = playerService;
			NavigationService = navigationService;
			SelectedAction = Runtime.EnemyTurnAction.None();
		}

		public void SelectAction(Runtime.EnemyTurnAction action)
		{
			SelectedAction = action;
		}

		public void ResetAction()
		{
			SelectedAction = Runtime.EnemyTurnAction.None();
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
	}
}
