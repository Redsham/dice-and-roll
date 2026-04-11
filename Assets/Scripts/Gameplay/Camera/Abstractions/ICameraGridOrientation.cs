using UnityEngine;


namespace Gameplay.Camera.Abstractions
{
	public interface ICameraGridOrientation
	{
		int        QuarterTurns { get; }
		Vector2Int RotateLocalDirectionToWorld(Vector2Int localDirection);
	}
}