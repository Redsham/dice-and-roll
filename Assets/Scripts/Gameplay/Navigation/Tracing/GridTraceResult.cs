using UnityEngine;


namespace Gameplay.Navigation.Tracing
{
	public readonly struct GridTraceResult
	{
		public int            Distance     { get; }
		public bool           Hit          { get; }
		public bool           ExitedBounds { get; }
		public Vector2Int     Point        { get; }
		public INavCellEntity Entity       { get; }

		public GridTraceResult(int distance, bool hit, bool exitedBounds, Vector2Int point, INavCellEntity entity = null)
		{
			Distance     = distance;
			Hit          = hit;
			ExitedBounds = exitedBounds;
			Point        = point;
			Entity       = entity;
		}
	}
}
