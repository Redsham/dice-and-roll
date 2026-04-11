using Gameplay.Navigation;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.World.Runtime;


namespace Gameplay.Player.Presentation.Combat
{
	public readonly struct DiceShotPresentationRequest
	{
		public DiceOrientation    Orientation { get; }
		public DiceFace           Face       { get; }
		public RollDirection      Direction  { get; }
		public int                ShotCount  { get; }
		public float              BurstDelay { get; }
		public GridBasis          GridBasis  { get; }
		public NavLineTraceStep[] TraceSteps { get; }

		public DiceShotPresentationRequest(
			DiceOrientation    orientation,
			DiceFace           face,
			RollDirection      direction,
			int                shotCount,
			float              burstDelay,
			GridBasis          gridBasis,
			NavLineTraceStep[] traceSteps
		)
		{
			Orientation = orientation;
			Face       = face;
			Direction  = direction;
			ShotCount  = shotCount;
			BurstDelay = burstDelay;
			GridBasis  = gridBasis;
			TraceSteps = traceSteps;
		}
	}
}
