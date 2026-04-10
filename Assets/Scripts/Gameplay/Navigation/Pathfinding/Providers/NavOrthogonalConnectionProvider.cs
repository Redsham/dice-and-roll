namespace Gameplay.Navigation.Pathfinding.Providers
{
	public struct NavOrthogonalConnectionProvider : INavConnectionProvider
	{
		// === Constants ===

		private const int STRAIGHT_COST = 10;

		// === Properties ===

		public int MaxConnectionCount => 4;

		// === API ===

		public int EstimateCost(in NavGrid grid, int fromIndex, int goalIndex)
		{
			int width = grid.Width;
			int fromX = fromIndex % width;
			int fromY = fromIndex / width;
			int goalX = goalIndex % width;
			int goalY = goalIndex / width;

			int dx = UnityEngine.Mathf.Abs(fromX - goalX);
			int dy = UnityEngine.Mathf.Abs(fromY - goalY);
			return (dx + dy) * STRAIGHT_COST;
		}

		public int Collect(in NavGrid grid, int nodeIndex, NavConnection[] buffer)
		{
			int width = grid.Width;
			int x     = nodeIndex % width;
			int y     = nodeIndex / width;
			int count = 0;

			if (x > 0) {
				int leftIndex = nodeIndex - 1;
				if (grid.Nodes[leftIndex].IsWalkable) {
					buffer[count++] = new(leftIndex, STRAIGHT_COST);
				}
			}

			if (x + 1 < width) {
				int rightIndex = nodeIndex + 1;
				if (grid.Nodes[rightIndex].IsWalkable) {
					buffer[count++] = new(rightIndex, STRAIGHT_COST);
				}
			}

			if (y > 0) {
				int upIndex = nodeIndex - width;
				if (grid.Nodes[upIndex].IsWalkable) {
					buffer[count++] = new(upIndex, STRAIGHT_COST);
				}
			}

			if (y + 1 < grid.Height) {
				int downIndex = nodeIndex + width;
				if (grid.Nodes[downIndex].IsWalkable) {
					buffer[count++] = new(downIndex, STRAIGHT_COST);
				}
			}

			return count;
		}
	}
}