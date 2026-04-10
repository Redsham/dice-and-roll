namespace Gameplay.Navigation.Pathfinding
{
	public readonly struct NavConnection
	{
		// === Data ===

		public int TargetIndex { get; }
		public int BaseCost    { get; }

		// === Lifecycle ===

		public NavConnection(int targetIndex, int baseCost)
		{
			TargetIndex = targetIndex;
			BaseCost    = baseCost;
		}
	}
}