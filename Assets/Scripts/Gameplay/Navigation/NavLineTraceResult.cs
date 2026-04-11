using UnityEngine;


namespace Gameplay.Navigation
{
	public readonly struct NavLineTraceResult
	{
		public int       StepCount      { get; }
		public int       RemainingPower { get; }
		public bool      WasStopped     { get; }
		public bool      ExitedBounds   { get; }
		public Vector2Int FinalCell     { get; }

		public NavLineTraceResult(int stepCount, int remainingPower, bool wasStopped, bool exitedBounds, Vector2Int finalCell)
		{
			StepCount      = stepCount;
			RemainingPower = remainingPower;
			WasStopped     = wasStopped;
			ExitedBounds   = exitedBounds;
			FinalCell      = finalCell;
		}
	}
}
