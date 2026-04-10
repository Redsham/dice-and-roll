using UnityEngine;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;


namespace Gameplay.Camera.Modes
{
	public sealed class OrbitCameraMode : ICameraMode, IOrbitCameraControl
	{
		private readonly OrbitCameraSettings m_Settings;

		private Vector3 m_FocusPosition;
		private Vector3 m_FocusVelocity;
		private float   m_CurrentYaw;
		private float   m_YawVelocity;
		private int     m_QuarterTurns;
		private bool    m_HasFocus;

		public OrbitCameraMode(OrbitCameraSettings settings)
		{
			m_Settings = settings;
			m_CurrentYaw = settings.Yaw;
		}

		public int QuarterTurns => m_QuarterTurns;

		public void OnEnter(in CameraModeContext context)
		{
			if (context.Target == null) {
				m_HasFocus = false;
				return;
			}

			m_FocusPosition = context.Target.position;
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

		public CameraPose Evaluate(float deltaTime, in CameraModeContext context)
		{
			if (context.Target == null) {
				return context.CurrentPose;
			}

			Vector3 targetPosition = context.Target.position;
			if (!m_HasFocus) {
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

			Vector3 planarOffset = Quaternion.Euler(0.0f, m_CurrentYaw, 0.0f) * (Vector3.back * m_Settings.PlanarDistance);
			Vector3 desiredPosition = new Vector3(
				m_FocusPosition.x + planarOffset.x,
				m_Settings.CameraHeight,
				m_FocusPosition.z + planarOffset.z
			);
			Vector3 lookPoint = m_FocusPosition + (Vector3.up * m_Settings.LookHeight);
			Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - desiredPosition, Vector3.up);

			return new CameraPose(desiredPosition, desiredRotation);
		}
	}
}
