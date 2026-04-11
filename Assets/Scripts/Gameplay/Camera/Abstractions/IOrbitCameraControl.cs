namespace Gameplay.Camera.Abstractions
{
	public interface IOrbitCameraControl
	{
		int  QuarterTurns { get; }
		void RotateLeft();
		void RotateRight();
	}
}