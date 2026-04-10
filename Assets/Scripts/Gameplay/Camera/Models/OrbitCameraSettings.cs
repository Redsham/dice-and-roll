using System;
using UnityEngine;


namespace Gameplay.Camera.Models
{
	[Serializable]
	public struct OrbitCameraSettings
	{
		[Min(0.0f)] public float PlanarDistance;
		public float CameraHeight;
		public float LookHeight;
		public float Yaw;
		[Min(0.01f)] public float FollowSmoothTime;
		[Min(0.01f)] public float RotationSmoothTime;

		public static OrbitCameraSettings Default => new() {
			PlanarDistance = 7.0f,
			CameraHeight = 6.0f,
			LookHeight = 1.0f,
			Yaw = 35.0f,
			FollowSmoothTime = 0.18f,
			RotationSmoothTime = 0.16f
		};
	}
}
