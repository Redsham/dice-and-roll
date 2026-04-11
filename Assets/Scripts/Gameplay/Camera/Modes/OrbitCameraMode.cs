using UnityEngine;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;


	namespace Gameplay.Camera.Modes
{
	public sealed class OrbitCameraMode : ICameraMode, IOrbitCameraControl, IOrbitCameraZoomControl
	{
		private readonly OrbitCameraSettings m_Settings;

		private Vector3 m_FocusPosition;
		private Vector3 m_FocusVelocity;
		private float   m_FocusHeight;
		private float   m_CurrentYaw;
		private float   m_YawVelocity;
		private float   m_CurrentZoomDistance;
		private float   m_TargetZoomDistance;
		private float   m_ZoomVelocity;
		private int     m_QuarterTurns;
		private bool    m_HasFocus;

		public OrbitCameraMode(OrbitCameraSettings settings)
		{
			m_Settings = settings;
			m_CurrentYaw          = settings.Yaw;
			m_CurrentZoomDistance = settings.PlanarDistance;
			m_TargetZoomDistance  = settings.PlanarDistance;
		}

		public int QuarterTurns => m_QuarterTurns;

		public void OnEnter(in CameraModeContext context)
		{
			if (context.Target == null) {
				m_HasFocus = false;
				return;
			}

			m_FocusHeight   = context.Target.position.y;
			m_FocusPosition = GetPlanarTargetPosition(context.Target.position);
			m_HasFocus      = true;
		}

		public void OnExit()
		{
		}

		public void RotateLeft()
		{
			m_QuarterTurns--;
		}

		public void RotateRight()
		{
			m_QuarterTurns++;
		}

		public void AddZoomInput(float delta)
		{
			if (Mathf.Approximately(delta, 0.0f)) {
				return;
			}

			float zoomStep = Mathf.Max(0.01f, m_Settings.ZoomInputStep);
			m_TargetZoomDistance = Mathf.Clamp(
				m_TargetZoomDistance - (delta * zoomStep),
				m_Settings.MinZoomDistance,
				m_Settings.MaxZoomDistance
			);
		}

		public CameraPose Evaluate(float deltaTime, in CameraModeContext context)
		{
			if (context.Target == null) {
				return context.CurrentPose;
			}

			Vector3 targetPosition = GetPlanarTargetPosition(context.Target.position);
			if (!m_HasFocus) {
				m_FocusHeight   = context.Target.position.y;
				m_FocusPosition = targetPosition;
				m_HasFocus = true;
			}

			m_FocusPosition = Vector3.SmoothDamp(
				m_FocusPosition,
				targetPosition,
				ref m_FocusVelocity,
				m_Settings.FollowSmoothTime,
				Mathf.Infinity,
				deltaTime
			);

			float targetYaw = m_Settings.Yaw + (90.0f * m_QuarterTurns);
			m_CurrentYaw = Mathf.SmoothDampAngle(
				m_CurrentYaw,
				targetYaw,
				ref m_YawVelocity,
				m_Settings.RotationSmoothTime,
				Mathf.Infinity,
				deltaTime
			);

			m_CurrentZoomDistance = Mathf.SmoothDamp(
				m_CurrentZoomDistance,
				m_TargetZoomDistance,
				ref m_ZoomVelocity,
				m_Settings.ZoomSmoothTime,
				Mathf.Infinity,
				deltaTime
			);

			float zoomLerp = Mathf.InverseLerp(m_Settings.MaxZoomDistance, m_Settings.MinZoomDistance, m_CurrentZoomDistance);
			float pitch = Mathf.Lerp(m_Settings.FarPitch, m_Settings.NearPitch, zoomLerp);
			Vector3 localOffset = Quaternion.Euler(pitch, m_CurrentYaw, 0.0f) * (Vector3.back * m_CurrentZoomDistance);
			Vector3 desiredPosition = m_FocusPosition + localOffset;
			Vector3 lookPoint = m_FocusPosition + (Vector3.up * m_Settings.LookHeight);
			Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - desiredPosition, Vector3.up);

			return new CameraPose(desiredPosition, desiredRotation);
		}

		private Vector3 GetPlanarTargetPosition(Vector3 targetPosition)
		{
			targetPosition.y = m_FocusHeight;
			return targetPosition;
		}
	}
}
