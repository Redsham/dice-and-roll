using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
namespace Gameplay.Player.Presentation.Combat
{
	public readonly struct DiceShotPresentationRequest
	{
		public DiceOrientation Orientation { get; }
		public DiceFace        Face        { get; }
		public int             TotalShots  { get; }

		public DiceShotPresentationRequest(
			DiceOrientation orientation,
			DiceFace        face,
			int             totalShots
		)
		{
			Orientation = orientation;
			Face        = face;
			TotalShots  = totalShots;
		}
	}
}
