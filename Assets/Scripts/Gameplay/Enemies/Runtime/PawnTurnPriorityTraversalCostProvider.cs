using Gameplay.Navigation;
using Gameplay.Navigation.Pathfinding.Providers;
using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Enemies.Runtime
{
	public struct PawnTurnPriorityTraversalCostProvider : INavTraversalCostProvider
	{
		private const int TURN_PENALTY = 3;

		public RollDirection InitialFacing { get; set; }

		public bool TryGetTraversalCost(in NavGrid grid, int previousIndex, int fromIndex, int toIndex, int baseCost, out int traversalCost)
		{
			traversalCost = baseCost;
			if (previousIndex < 0) {
				Vector2Int initialStep = grid.ToCoordinates(toIndex) - grid.ToCoordinates(fromIndex);
				if (TryGetDirection(initialStep, out RollDirection initialDirection) && initialDirection != InitialFacing) {
					traversalCost += TURN_PENALTY;
				}

				return true;
			}

			int previousDeltaX = fromIndex % grid.Width - previousIndex % grid.Width;
			int previousDeltaY = fromIndex / grid.Width - previousIndex / grid.Width;
			int nextDeltaX     = toIndex % grid.Width - fromIndex % grid.Width;
			int nextDeltaY     = toIndex / grid.Width - fromIndex / grid.Width;

			if (previousDeltaX != nextDeltaX || previousDeltaY != nextDeltaY) {
				traversalCost += TURN_PENALTY;
			}

			return true;
		}

		private static bool TryGetDirection(Vector2Int step, out RollDirection direction)
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
