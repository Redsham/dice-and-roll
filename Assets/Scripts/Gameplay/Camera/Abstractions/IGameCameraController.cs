using UnityEngine;
using Gameplay.Camera.Models;


namespace Gameplay.Camera.Abstractions
{
	public interface IGameCameraController
	{
		public Vector3    Position { get; }
		public Quaternion Rotation { get; }

		void FollowWithOrbit(Transform target, float     blendDuration = -1.0f);
		void FocusOnWorldPoint(Transform target, Vector3 worldPoint, float blendDuration = -1.0f);
		void FocusOnTrackedTransform(Transform target, Transform trackedTransform, float blendDuration = -1.0f);
		void SetMode(ICameraMode       mode,   Transform target        = null, float blendDuration = -1.0f);
		void ClearTarget();
		void RotateOrbitLeft();
		void RotateOrbitRight();
		void AdjustOrbitZoom(float     delta);
		void SetFieldOfView(float fieldOfView, float smoothTime = 0.2f);
		void Shake(CameraShakeSettings settings);
	}
}
