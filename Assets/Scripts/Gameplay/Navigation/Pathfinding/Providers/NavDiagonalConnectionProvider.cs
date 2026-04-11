using UnityEngine;


namespace Gameplay.Navigation.Pathfinding.Providers
{
	public struct NavDiagonalConnectionProvider : INavConnectionProvider
	{
		// === Constants ===

		private const int STRAIGHT_COST = 10;
		private const int DIAGONAL_COST = 14;

		// === Properties ===

		public int MaxConnectionCount => 8;

		// === API ===

		public int EstimateCost(in NavGrid grid, int fromIndex, int goalIndex)
		{
			int width = grid.Width;
			int fromX = fromIndex % width;
			int fromY = fromIndex / width;
			int goalX = goalIndex % width;
			int goalY = goalIndex / width;

			int dx       = Mathf.Abs(fromX - goalX);
			int dy       = Mathf.Abs(fromY - goalY);
			int diagonal = Mathf.Min(dx, dy);
			int straight = Mathf.Abs(dx - dy);
			return diagonal * DIAGONAL_COST + straight * STRAIGHT_COST;
		}

		public int Collect(in NavGrid grid, int nodeIndex, NavConnection[] buffer)
		{
			int width = grid.Width;
			int x     = nodeIndex % width;
			int y     = nodeIndex / width;
			int count = 0;

			bool canMoveLeft  = false;
			bool canMoveRight = false;
			bool canMoveUp    = false;
			bool canMoveDown  = false;

			if (x > 0) {
				int leftIndex = nodeIndex - 1;
				canMoveLeft = grid.Nodes[leftIndex].CanOccupy;
				if (canMoveLeft) {
					buffer[count++] = new(leftIndex, STRAIGHT_COST);
				}
			}

			if (x + 1 < width) {
				int rightIndex = nodeIndex + 1;
				canMoveRight = grid.Nodes[rightIndex].CanOccupy;
				if (canMoveRight) {
					buffer[count++] = new(rightIndex, STRAIGHT_COST);
				}
			}

			if (y > 0) {
				int upIndex = nodeIndex - width;
				canMoveUp = grid.Nodes[upIndex].CanOccupy;
				if (canMoveUp) {
					buffer[count++] = new(upIndex, STRAIGHT_COST);
				}
			}

			if (y + 1 < grid.Height) {
				int downIndex = nodeIndex + width;
				canMoveDown = grid.Nodes[downIndex].CanOccupy;
				if (canMoveDown) {
					buffer[count++] = new(downIndex, STRAIGHT_COST);
				}
			}

			if (x > 0 && y > 0 && grid.Nodes[nodeIndex - width - 1].CanOccupy && CanTraverseDiagonal(grid, canMoveLeft, canMoveUp)) {
				buffer[count++] = new(nodeIndex - width - 1, DIAGONAL_COST);
			}

			if (x + 1 < width && y > 0 && grid.Nodes[nodeIndex - width + 1].CanOccupy && CanTraverseDiagonal(grid, canMoveRight, canMoveUp)) {
				buffer[count++] = new(nodeIndex - width + 1, DIAGONAL_COST);
			}

			if (x > 0 && y + 1 < grid.Height && grid.Nodes[nodeIndex + width - 1].CanOccupy && CanTraverseDiagonal(grid, canMoveLeft, canMoveDown)) {
				buffer[count++] = new(nodeIndex + width - 1, DIAGONAL_COST);
			}

			if (x + 1 < width && y + 1 < grid.Height && grid.Nodes[nodeIndex + width + 1].CanOccupy && CanTraverseDiagonal(grid, canMoveRight, canMoveDown)) {
				buffer[count++] = new(nodeIndex + width + 1, DIAGONAL_COST);
			}

			return count;
		}

		// === Helpers ===

		private static bool CanTraverseDiagonal(in NavGrid grid, bool firstAxisOpen, bool secondAxisOpen)
		{
			return grid.AllowCornerCutting || firstAxisOpen && secondAxisOpen;
		}
	}
}