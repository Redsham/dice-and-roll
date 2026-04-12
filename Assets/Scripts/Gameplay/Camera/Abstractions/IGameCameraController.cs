using UnityEngine;
using Gameplay.Camera.Models;


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
		void Shake(CameraShakeSettings settings);
	}
}
