using UnityEngine;


namespace Gameplay.Camera.Abstractions
{
	public interface IGameCameraController
	{
		void FollowWithOrbit(Transform target, float     blendDuration = -1.0f);
		void SetMode(ICameraMode       mode,   Transform target        = null, float blendDuration = -1.0f);
		void ClearTarget();
		void RotateOrbitLeft();
		void RotateOrbitRight();
		void AdjustOrbitZoom(float delta);
		void Shake(float           amplitude, float duration, float frequency = 25.0f, float rotationalAmplitude = 1.0f);
	}
}