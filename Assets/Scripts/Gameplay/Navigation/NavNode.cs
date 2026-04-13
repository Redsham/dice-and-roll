namespace Gameplay.Navigation
{
	public struct NavNode
	{
		// === Data ===

		public readonly int            Index;
		public          bool           IsWalkable;
		public          INavCellEntity Tile;
		public          INavCellEntity Actor;
		public readonly INavCellEntity Entity => Actor ?? Tile;
		public          bool           CanOccupy => IsWalkable && Actor == null && (Tile == null || Tile.Flags.HasFlag(NavCellFlags.Walkable));

		// === Lifecycle ===

		public NavNode(int index, bool isWalkable)
		{
			Index      = index;
			IsWalkable = isWalkable;
			Tile       = null;
			Actor      = null;
		}
	}
}
