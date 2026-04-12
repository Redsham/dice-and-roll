using UnityEngine;


namespace Gameplay.Navigation
{
	public readonly struct GridTraceResult
	{
		public int        Distance     { get; }
		public bool       Hit          { get; }
		public bool       ExitedBounds { get; }
		public Vector2Int Point        { get; }

		public GridTraceResult(int distance, bool hit, bool exitedBounds, Vector2Int point)
		{
			Distance     = distance;
			Hit          = hit;
			ExitedBounds = exitedBounds;
			Point        = point;
		}
	}
}