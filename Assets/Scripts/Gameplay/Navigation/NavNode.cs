namespace Gameplay.Navigation
{
	public struct NavNode
	{
		// === Data ===

		public readonly int              Index;
		public          bool             IsWalkable;
		public          NavCellOccupancy Occupancy;
		public          bool             CanOccupy => IsWalkable && !Occupancy.BlocksMovement;

		// === Lifecycle ===

		public NavNode(int index, bool isWalkable)
		{
			Index      = index;
			IsWalkable = isWalkable;
			Occupancy  = NavCellOccupancy.Empty;
		}
	}
}