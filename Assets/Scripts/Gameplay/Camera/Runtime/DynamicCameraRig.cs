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
		[SerializeField]              private UnityEngine.Camera  m_Camera;
		[SerializeField]              private OrbitCameraSettings m_DefaultOrbit;
		[SerializeField, Min(0.0f)]   private float               m_DefaultBlendDuration     = 0.35f;
		[SerializeField, Range(0, 3)] private int                 m_ControlQuarterTurnOffset = 1;

		private ICameraMode m_CurrentMode;
		private Transform   m_CurrentTarget;

		private CameraPose m_CurrentPose;
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

		private void Awake()
		{
			if (m_Camera == null) {
				m_Camera = GetComponent<UnityEngine.Camera>();
			}

			if (m_DefaultOrbit.PlanarDistance <= 0.0f) {
				m_DefaultOrbit = OrbitCameraSettings.Default;
			}
			else {
				if (m_DefaultOrbit.FollowSmoothTime <= 0.0f) {
					m_DefaultOrbit.FollowSmoothTime = OrbitCameraSettings.Default.FollowSmoothTime;
				}

				if (m_DefaultOrbit.RotationSmoothTime <= 0.0f) {
					m_DefaultOrbit.RotationSmoothTime = OrbitCameraSettings.Default.RotationSmoothTime;
				}

				if (m_DefaultOrbit.ZoomSmoothTime <= 0.0f) {
					m_DefaultOrbit.ZoomSmoothTime = OrbitCameraSettings.Default.ZoomSmoothTime;
				}

				if (m_DefaultOrbit.MinZoomDistance <= 0.0f) {
					m_DefaultOrbit.MinZoomDistance = OrbitCameraSettings.Default.MinZoomDistance;
				}

				if (m_DefaultOrbit.MaxZoomDistance <= 0.0f) {
					m_DefaultOrbit.MaxZoomDistance = OrbitCameraSettings.Default.MaxZoomDistance;
				}

				if (m_DefaultOrbit.MaxZoomDistance < m_DefaultOrbit.MinZoomDistance) {
					m_DefaultOrbit.MaxZoomDistance = m_DefaultOrbit.MinZoomDistance;
				}

				if (m_DefaultOrbit.ZoomInputStep <= 0.0f) {
					m_DefaultOrbit.ZoomInputStep = OrbitCameraSettings.Default.ZoomInputStep;
				}
			}

			m_CurrentPose = new(transform.position, transform.rotation);
			m_ShakeSeed   = new(Random.value * 100.0f, Random.value * 100.0f);
		}

		private void LateUpdate()
		{
			float      deltaTime   = Time.deltaTime;
			CameraPose desiredPose = EvaluatePose(deltaTime);
			CameraPose finalPose   = ApplyBlend(desiredPose, deltaTime);
			finalPose = ApplyShake(finalPose);

			m_CurrentPose = finalPose;
			transform.SetPositionAndRotation(finalPose.Position, finalPose.Rotation);
		}

		public void FollowWithOrbit(Transform target, float blendDuration = -1.0f)
		{
			SetMode(new OrbitCameraMode(m_DefaultOrbit), target, blendDuration);
		}

		public void SetMode(ICameraMode mode, Transform target = null, float blendDuration = -1.0f)
		{
			if (mode == null) {
				throw new ArgumentNullException(nameof(mode));
			}

			m_CurrentMode?.OnExit();

			m_CurrentMode   = mode;
			m_CurrentTarget = target;
			m_CurrentMode.OnEnter(new(m_CurrentTarget, m_CurrentPose));

			m_BlendStartPose = m_CurrentPose;
			m_BlendElapsed   = 0.0f;
			m_BlendDuration  = blendDuration >= 0.0f ? blendDuration : m_DefaultBlendDuration;
			m_IsBlending     = m_BlendDuration > 0.0f;
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

		public void AdjustOrbitZoom(float delta)
		{
			if (m_CurrentMode is IOrbitCameraZoomControl orbitCameraZoomControl) {
				orbitCameraZoomControl.AddZoomInput(delta);
			}
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
			m_ShakeFrequency           = Mathf.Max(0.01f, settings.Frequency);
			m_ShakeRotationalAmplitude = Mathf.Max(m_ShakeRotationalAmplitude, settings.RotationalAmplitude);
		}

		public bool TryProjectScreenPointToPlane(Vector2 screenPosition, Vector3 planeOrigin, Vector3 planeNormal, out Vector3 worldPoint)
		{
			Ray   ray   = m_Camera.ScreenPointToRay(screenPosition);
			Plane plane = new(planeNormal, planeOrigin);
			if (plane.Raycast(ray, out float distance)) {
				worldPoint = ray.GetPoint(distance);
				return true;
			}

			worldPoint = default;
			return false;
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
			float t      = Mathf.Clamp01(m_BlendElapsed / m_BlendDuration);
			float easedT = t * t * (3.0f - 2.0f         * t);

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
	}
}
