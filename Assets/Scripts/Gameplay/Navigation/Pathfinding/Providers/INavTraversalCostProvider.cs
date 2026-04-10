namespace Gameplay.Navigation.Pathfinding.Providers
{
	public interface INavTraversalCostProvider
	{
		// Returns false when the edge should be treated as blocked.
		bool TryGetTraversalCost(in NavGrid grid, int fromIndex, int toIndex, int baseCost, out int traversalCost);
	}
}