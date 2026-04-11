namespace Gameplay.Navigation.Pathfinding.Providers
{
	public struct NavDefaultTraversalCostProvider : INavTraversalCostProvider
	{
		public bool TryGetTraversalCost(in NavGrid grid, int previousIndex, int fromIndex, int toIndex, int baseCost, out int traversalCost)
		{
			traversalCost = baseCost;
			return true;
		}
	}
}
