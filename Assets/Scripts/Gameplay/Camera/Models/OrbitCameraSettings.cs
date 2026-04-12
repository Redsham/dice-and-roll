using System;
using UnityEngine;


namespace Gameplay.Camera.Models
{
	[Serializable]
	public struct CameraShakeSettings
	{
		[Min(0.0f)]  public float Amplitude;
		[Min(0.0f)]  public float Duration;
		[Min(0.01f)] public float Frequency;
		[Min(0.0f)]  public float RotationalAmplitude;

		public bool IsEnabled => Amplitude > 0.0f && Duration > 0.0f;

		public static CameraShakeSettings Default => new() {
			Amplitude           = 0.2f,
			Duration            = 0.18f,
			Frequency           = 28.0f,
			RotationalAmplitude = 1.2f
		};
	}

	[Serializable]
	public struct OrbitCameraSettings
	{
		[Min(0.0f)] public  float PlanarDistance;
		public              float CameraHeight;
		public              float LookHeight;
		public              float Yaw;
		[Min(0.01f)] public float FollowSmoothTime;
		[Min(0.01f)] public float RotationSmoothTime;
		[Min(0.01f)] public float ZoomSmoothTime;
		[Min(0.01f)] public float MinZoomDistance;
		[Min(0.01f)] public float MaxZoomDistance;
		public              float FarPitch;
		public              float NearPitch;
		[Min(0.01f)] public float ZoomInputStep;

		public static OrbitCameraSettings Default => new() {
			PlanarDistance     = 7.0f,
			CameraHeight       = 6.0f,
			LookHeight         = 1.0f,
			Yaw                = 35.0f,
			FollowSmoothTime   = 0.18f,
			RotationSmoothTime = 0.16f,
			ZoomSmoothTime     = 0.2f,
			MinZoomDistance    = 3.5f,
			MaxZoomDistance    = 9.0f,
			FarPitch           = 42.0f,
			NearPitch          = 20.0f,
			ZoomInputStep      = 1.0f
		};
	}
}
