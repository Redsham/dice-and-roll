namespace Gameplay.Navigation.Pathfinding
{
	public enum NavPathStatus
	{
		Found,
		NoPath,
		InvalidStart,
		InvalidGoal,
		StartBlocked,
		GoalBlocked,
		BufferTooSmall
	}

	public readonly struct NavPathResult
	{
		// === Data ===

		public NavPathStatus Status       { get; }
		public int           PathLength   { get; }
		public int           TotalCost    { get; }
		public int           RequiredSize { get; }

		public bool HasPath => Status == NavPathStatus.Found;

		// === Lifecycle ===

		public NavPathResult(NavPathStatus status, int pathLength, int totalCost, int requiredSize)
		{
			Status       = status;
			PathLength   = pathLength;
			TotalCost    = totalCost;
			RequiredSize = requiredSize;
		}
	}
}