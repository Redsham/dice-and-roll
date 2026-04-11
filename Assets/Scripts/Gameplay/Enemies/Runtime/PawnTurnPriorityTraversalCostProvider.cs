using Gameplay.Navigation;
using Gameplay.Navigation.Pathfinding.Providers;


namespace Gameplay.Enemies.Runtime
{
	public struct PawnTurnPriorityTraversalCostProvider : INavTraversalCostProvider
	{
		private const int TURN_PENALTY = 3;

		public bool TryGetTraversalCost(in NavGrid grid, int previousIndex, int fromIndex, int toIndex, int baseCost, out int traversalCost)
		{
			traversalCost = baseCost;
			if (previousIndex < 0) {
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
	}
}
