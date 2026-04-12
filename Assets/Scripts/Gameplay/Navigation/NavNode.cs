namespace Gameplay.Navigation
{
	public struct NavNode
	{
		// === Data ===

		public readonly int            Index;
		public          bool           IsWalkable;
		public          INavCellEntity Entity;
		public          bool           CanOccupy => IsWalkable && (Entity == null || Entity.Flags.HasFlag(NavCellFlags.Walkable));

		// === Lifecycle ===

		public NavNode(int index, bool isWalkable)
		{
			Index      = index;
			IsWalkable = isWalkable;
			Entity     = null;
		}
	}
}
