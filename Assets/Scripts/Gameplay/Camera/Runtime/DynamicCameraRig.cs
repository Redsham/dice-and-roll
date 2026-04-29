using System;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
using Gameplay.Camera.Modes;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Gameplay.Camera.Runtime
{
	[DisallowMultipleComponent]
	public sealed class DynamicCameraRig : MonoBehaviour, IGameCameraController, ICameraGridOrientation, ICameraScreenProjector
	{
		private const float DEFAULT_FIELD_OF_VIEW_SMOOTH_TIME = 0.2f;

		public Vector3    Position => transform.position;
		public Quaternion Rotation => transform.rotation;

		[SerializeField]              private UnityEngine.Camera  m_Camera;
		[SerializeField]              private OrbitCameraSettings m_DefaultOrbit;
		[SerializeField]              private FocusWorldPointCameraSettings m_DefaultFocusWorldPoint = FocusWorldPointCameraSettings.Default;
		[SerializeField]              private FocusTrackedTransformCameraSettings m_DefaultFocusTrackedTransform = FocusTrackedTransformCameraSettings.Default;
		[SerializeField, Min(0.0f)]   private float               m_DefaultBlendDuration     = 0.35f;
		[SerializeField, Range(0, 3)] private int                 m_ControlQuarterTurnOffset = 1;

		private ICameraMode m_CurrentMode;
		private Transform   m_CurrentTarget;

		private CameraPose m_CurrentPose;
		private CameraPose m_RenderPose;
		private CameraPose m_BlendStartPose;
		private float      m_BlendDuration;
		private float      m_BlendElapsed;
		private bool       m_IsBlending;

		private float   m_ShakeAmplitude;
		private float   m_ShakeDuration;
		private float   m_ShakeElapsed;
		private float   m_ShakeFrequency;
		private float   m_ShakeRotationalAmplitude;
		private Vector2 m_ShakeSeed;
		private float   m_DefaultFieldOfView;
		private float   m_TargetFieldOfView;
		private float   m_FieldOfViewVelocity;
		private float   m_FieldOfViewSmoothTime = DEFAULT_FIELD_OF_VIEW_SMOOTH_TIME;

		private void Awake()
		{
			if (m_Camera == null) {
				m_Camera = GetComponent<UnityEngine.Camera>();
			}

			m_DefaultOrbit = SanitizeOrbitSettings(m_DefaultOrbit);

			m_CurrentPose        = new(transform.position, transform.rotation);
			m_RenderPose         = m_CurrentPose;
			m_ShakeSeed          = new(Random.value * 100.0f, Random.value * 100.0f);
			m_DefaultFieldOfView = m_Camera != null ? m_Camera.fieldOfView : 60.0f;
			m_TargetFieldOfView  = m_DefaultFieldOfView;
		}

		private void LateUpdate()
		{
			float      deltaTime   = Time.deltaTime;
			CameraPose desiredPose = EvaluatePose(deltaTime);
			CameraPose blendedPose = ApplyBlend(desiredPose, deltaTime);
			CameraPose finalPose   = ApplyShake(blendedPose);

			m_CurrentPose = blendedPose;
			m_RenderPose  = finalPose;
			transform.SetPositionAndRotation(finalPose.Position, finalPose.Rotation);
			UpdateFieldOfView(deltaTime);
		}

		public void FollowWithOrbit(Transform target, float blendDuration = -1.0f)
		{
			RestoreDefaultFieldOfView();
			SetMode(new OrbitCameraMode(m_DefaultOrbit), target, blendDuration);
		}

		public void FocusOnWorldPoint(Transform target, Vector3 worldPoint, float blendDuration = -1.0f)
		{
			RestoreDefaultFieldOfView();
			SetMode(new FocusWorldPointCameraMode(worldPoint, m_DefaultFocusWorldPoint), target, blendDuration);
		}

		public void FocusOnTrackedTransform(Transform target, Transform trackedTransform, float blendDuration = -1.0f)
		{
			SetTargetFieldOfView(m_DefaultFocusTrackedTransform.TargetFieldOfView, m_DefaultFocusTrackedTransform.FieldOfViewSmoothTime);
			SetMode(new FocusTrackedTransformCameraMode(trackedTransform, m_DefaultFocusTrackedTransform), target, blendDuration);
		}

		public void SetMode(ICameraMode mode, Transform target = null, float blendDuration = -1.0f)
		{
			if (mode == null) {
				throw new ArgumentNullException(nameof(mode));
			}

			bool isFirstMode = m_CurrentMode == null;
			m_CurrentMode?.OnExit();

			m_CurrentMode   = mode;
			m_CurrentTarget = target;
			m_CurrentMode.OnEnter(new(m_CurrentTarget, m_CurrentPose));

			if (isFirstMode) {
				CameraPose initialPose = m_CurrentMode.Evaluate(0.0f, new(m_CurrentTarget, m_CurrentPose));
				m_CurrentPose = initialPose;
				transform.SetPositionAndRotation(initialPose.Position, initialPose.Rotation);
			}

			m_BlendStartPose = m_CurrentPose;
			m_BlendElapsed   = 0.0f;
			m_BlendDuration  = blendDuration >= 0.0f ? blendDuration : m_DefaultBlendDuration;
			m_IsBlending     = !isFirstMode && m_BlendDuration > 0.0f;
		}

		public void ClearTarget()
		{
			m_CurrentTarget = null;
		}

		public void RotateOrbitLeft()
		{
			if (m_CurrentMode is IOrbitCameraControl orbitCameraControl) {
				orbitCameraControl.RotateLeft();
			}
		}

		public void RotateOrbitRight()
		{
			if (m_CurrentMode is IOrbitCameraControl orbitCameraControl) {
				orbitCameraControl.RotateRight();
			}
		}

		public void SetOrbitRotationPreview(float yawOffset)
		{
			if (m_CurrentMode is IOrbitCameraControl orbitCameraControl) {
				orbitCameraControl.SetRotationPreview(yawOffset);
			}
		}

		public void AdjustOrbitZoom(float delta)
		{
			if (m_CurrentMode is IOrbitCameraZoomControl orbitCameraZoomControl) {
				orbitCameraZoomControl.AddZoomInput(delta);
			}
		}

		public void SetFieldOfView(float fieldOfView, float smoothTime = 0.2f)
		{
			SetTargetFieldOfView(fieldOfView, smoothTime);
		}

		public int QuarterTurns
		{
			get
			{
				if (m_CurrentMode is not IOrbitCameraControl orbitCameraControl) {
					return 0;
				}

				return NormalizeQuarterTurns(orbitCameraControl.QuarterTurns);
			}
		}

		public Vector2Int RotateLocalDirectionToWorld(Vector2Int localDirection)
		{
			int quarterTurns = NormalizeQuarterTurns(QuarterTurns + m_ControlQuarterTurnOffset);
			if (quarterTurns == 0) {
				return localDirection;
			}

			return quarterTurns switch {
				1 => new(localDirection.y, -localDirection.x),
				2 => new(-localDirection.x, -localDirection.y),
				3 => new(-localDirection.y, localDirection.x),
				_ => localDirection
			};
		}

		public void Shake(CameraShakeSettings settings)
		{
			if (!settings.IsEnabled) {
				return;
			}

			m_ShakeAmplitude           = Mathf.Max(m_ShakeAmplitude, settings.Amplitude);
			m_ShakeDuration            = Mathf.Max(m_ShakeDuration,  settings.Duration);
			m_ShakeElapsed             = 0.0f;
			m_ShakeFrequency           = Mathf.Max(0.01f,                      settings.Frequency);
			m_ShakeRotationalAmplitude = Mathf.Max(m_ShakeRotationalAmplitude, settings.RotationalAmplitude);
		}

		public bool TryCreateScreenRay(Vector2 screenPosition, out Ray ray)
		{
			if (m_Camera == null) {
				ray = default;
				return false;
			}

			ray = m_Camera.ScreenPointToRay(screenPosition);
			return true;
		}

		public bool TryProjectScreenPointToPlane(Vector2 screenPosition, Vector3 planeOrigin, Vector3 planeNormal, out Vector3 worldPoint)
		{
			if (!TryCreateScreenRay(screenPosition, out Ray ray)) {
				worldPoint = default;
				return false;
			}

			Plane plane = new(planeNormal, planeOrigin);
			if (plane.Raycast(ray, out float distance)) {
				worldPoint = ray.GetPoint(distance);
				return true;
			}

			worldPoint = default;
			return false;
		}

		public bool TryProjectWorldToScreenPoint(Vector3 worldPoint, out Vector2 screenPoint, out bool isBehindCamera)
		{
			if (m_Camera == null) {
				screenPoint    = default;
				isBehindCamera = true;
				return false;
			}

			Vector3 projectedPoint = m_Camera.WorldToScreenPoint(worldPoint);
			screenPoint    = projectedPoint;
			isBehindCamera = projectedPoint.z < 0.0f;
			return !isBehindCamera;
		}

		private CameraPose EvaluatePose(float deltaTime)
		{
			if (m_CurrentMode == null) {
				return m_CurrentPose;
			}

			CameraModeContext context = new(m_CurrentTarget, m_CurrentPose);
			return m_CurrentMode.Evaluate(deltaTime, context);
		}

		private CameraPose ApplyBlend(CameraPose desiredPose, float deltaTime)
		{
			if (!m_IsBlending) {
				return desiredPose;
			}

			m_BlendElapsed += deltaTime;
			float t = Mathf.Clamp01(m_BlendElapsed / m_BlendDuration);
			float easedT = t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);

			CameraPose blendedPose = CameraPose.Lerp(m_BlendStartPose, desiredPose, easedT);
			if (t >= 1.0f) {
				m_IsBlending = false;
			}

			return blendedPose;
		}

		private CameraPose ApplyShake(CameraPose pose)
		{
			if (m_ShakeElapsed >= m_ShakeDuration || m_ShakeAmplitude <= 0.0f) {
				return pose;
			}

			m_ShakeElapsed += Time.deltaTime;
			float normalizedTime = Mathf.Clamp01(m_ShakeElapsed / m_ShakeDuration);
			float attenuation    = 1.0f - normalizedTime;
			float strength       = m_ShakeAmplitude * attenuation;
			float time           = Time.time        * m_ShakeFrequency;

			Vector3 positionalOffset = new Vector3(
			                                       (Mathf.PerlinNoise(m_ShakeSeed.x,         time) - 0.5f) * 2.0f,
			                                       (Mathf.PerlinNoise(m_ShakeSeed.y,         time) - 0.5f) * 2.0f,
			                                       (Mathf.PerlinNoise(m_ShakeSeed.x + 37.0f, time) - 0.5f) * 2.0f
			                                      ) * strength;

			Vector3 rotationalOffset = new Vector3(
			                                       (Mathf.PerlinNoise(m_ShakeSeed.x + 101.0f, time) - 0.5f) * 2.0f,
			                                       (Mathf.PerlinNoise(m_ShakeSeed.y + 203.0f, time) - 0.5f) * 2.0f,
			                                       (Mathf.PerlinNoise(m_ShakeSeed.x + 307.0f, time) - 0.5f) * 2.0f
			                                      ) * (strength * m_ShakeRotationalAmplitude);

			Quaternion shakeRotation = Quaternion.Euler(rotationalOffset);
			return new(pose.Position + positionalOffset, pose.Rotation * shakeRotation);
		}

		private static int NormalizeQuarterTurns(int quarterTurns)
		{
			int normalizedQuarterTurns = quarterTurns % 4;
			return normalizedQuarterTurns < 0 ? normalizedQuarterTurns + 4 : normalizedQuarterTurns;
		}

		private static OrbitCameraSettings SanitizeOrbitSettings(OrbitCameraSettings settings)
		{
			if (settings.PlanarDistance <= 0.0f) {
				return OrbitCameraSettings.Default;
			}

			OrbitCameraSettings defaults = OrbitCameraSettings.Default;

			if (settings.FollowSmoothTime <= 0.0f) {
				settings.FollowSmoothTime = defaults.FollowSmoothTime;
			}

			if (settings.RotationSmoothTime <= 0.0f) {
				settings.RotationSmoothTime = defaults.RotationSmoothTime;
			}

			if (settings.ZoomSmoothTime <= 0.0f) {
				settings.ZoomSmoothTime = defaults.ZoomSmoothTime;
			}

			if (settings.MinZoomDistance <= 0.0f) {
				settings.MinZoomDistance = defaults.MinZoomDistance;
			}

			if (settings.MaxZoomDistance <= 0.0f) {
				settings.MaxZoomDistance = defaults.MaxZoomDistance;
			}

			if (settings.MaxZoomDistance < settings.MinZoomDistance) {
				settings.MaxZoomDistance = settings.MinZoomDistance;
			}

			if (settings.ZoomInputStep <= 0.0f) {
				settings.ZoomInputStep = defaults.ZoomInputStep;
			}

			return settings;
		}

		private void RestoreDefaultFieldOfView()
		{
			SetTargetFieldOfView(m_DefaultFieldOfView, DEFAULT_FIELD_OF_VIEW_SMOOTH_TIME);
		}

		private void SetTargetFieldOfView(float fieldOfView, float smoothTime)
		{
			m_TargetFieldOfView     = fieldOfView;
			m_FieldOfViewSmoothTime = Mathf.Max(0.01f, smoothTime);
		}

		private void UpdateFieldOfView(float deltaTime)
		{
			if (m_Camera == null) {
				return;
			}

			m_Camera.fieldOfView = Mathf.SmoothDamp(
				m_Camera.fieldOfView,
				m_TargetFieldOfView,
				ref m_FieldOfViewVelocity,
				m_FieldOfViewSmoothTime,
				Mathf.Infinity,
				deltaTime
			);
		}
	}
}
