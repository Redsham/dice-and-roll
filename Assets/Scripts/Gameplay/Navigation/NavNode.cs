namespace Gameplay.Navigation
{
	public struct NavNode
	{
		// === Data ===

		public readonly int  Index;
		public          bool IsWalkable;

		// === Lifecycle ===

		public NavNode(int index, bool isWalkable)
		{
			Index = index;
			IsWalkable = isWalkable;
		}
	}
}
