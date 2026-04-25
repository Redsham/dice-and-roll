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

	[Serializable]
	public struct FocusWorldPointCameraSettings
	{
		[Min(0.5f)] public float DistanceMultiplier;
		[Min(0.0f)] public float SideOffset;
		[Min(0.0f)] public float BackOffset;
		public float HeightOffset;
		[Range(0.0f, 1.0f)] public float FocusPointBlend;
		[Range(0.0f, 1.0f)] public float EventLookBlend;
		[Min(0.01f)] public float PositionLerpSpeed;
		[Min(0.01f)] public float LookLerpSpeed;

		public static FocusWorldPointCameraSettings Default => new() {
			DistanceMultiplier = 1.1f,
			SideOffset         = 0.85f,
			BackOffset         = 1.05f,
			HeightOffset       = 1.1f,
			FocusPointBlend    = 0.58f,
			EventLookBlend     = 0.72f,
			PositionLerpSpeed  = 6.0f,
			LookLerpSpeed      = 4.25f
		};
	}

	[Serializable]
	public struct FocusTrackedTransformCameraSettings
	{
		[Min(0.5f)] public float DistanceMultiplier;
		[Min(0.0f)] public float SideOffset;
		[Min(0.0f)] public float BackOffset;
		public float HeightOffset;
		[Min(0.0f)] public float PlanarParallax;
		[Min(0.0f)] public float VerticalParallax;
		[Range(0.0f, 1.0f)] public float AnchorLookBlend;
		[Range(0.0f, 1.0f)] public float TrackedLookBlend;
		[Min(0.01f)] public float PositionLerpSpeed;
		[Min(0.01f)] public float LookLerpSpeed;
		[Min(1.0f)] public float TargetFieldOfView;
		[Min(0.01f)] public float FieldOfViewSmoothTime;

		public static FocusTrackedTransformCameraSettings Default => new() {
			DistanceMultiplier  = 1.08f,
			SideOffset          = 0.95f,
			BackOffset          = 1.2f,
			HeightOffset        = 1.2f,
			PlanarParallax      = 0.1f,
			VerticalParallax    = 0.22f,
			AnchorLookBlend     = 0.35f,
			TrackedLookBlend    = 0.78f,
			PositionLerpSpeed   = 7.5f,
			LookLerpSpeed       = 4.6f,
			TargetFieldOfView   = 52.0f,
			FieldOfViewSmoothTime = 0.22f
		};
	}
}
