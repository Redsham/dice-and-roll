using UnityEngine;


namespace Gameplay.Navigation
{
	public readonly struct NavLineTraceStep
	{
		public Vector2Int       Coordinates   { get; }
		public NavCellOccupancy Occupancy     { get; }
		public int              Distance      { get; }
		public int              PowerBefore   { get; }
		public int              PowerConsumed { get; }
		public int              PowerAfter    { get; }
		public bool             StopsTrace    { get; }

		public NavLineTraceStep(
			Vector2Int       coordinates,
			NavCellOccupancy occupancy,
			int              distance,
			int              powerBefore,
			int              powerConsumed,
			int              powerAfter,
			bool             stopsTrace
		)
		{
			Coordinates   = coordinates;
			Occupancy     = occupancy;
			Distance      = distance;
			PowerBefore   = powerBefore;
			PowerConsumed = powerConsumed;
			PowerAfter    = powerAfter;
			StopsTrace    = stopsTrace;
		}
	}
}
