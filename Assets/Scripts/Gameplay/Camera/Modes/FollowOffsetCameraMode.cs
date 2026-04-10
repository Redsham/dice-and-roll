using UnityEngine;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;


namespace Gameplay.Camera.Modes
{
	public sealed class FollowOffsetCameraMode : ICameraMode
	{
		private readonly Vector3 m_WorldOffset;
		private readonly Vector3 m_LookOffset;
		private readonly float   m_PositionLerpSpeed;

		private Vector3 m_SmoothedTarget;
		private bool    m_HasTarget;

		public FollowOffsetCameraMode(Vector3 worldOffset, Vector3 lookOffset, float positionLerpSpeed = 8.0f)
		{
			m_WorldOffset = worldOffset;
			m_LookOffset = lookOffset;
			m_PositionLerpSpeed = Mathf.Max(0.01f, positionLerpSpeed);
		}

		public void OnEnter(in CameraModeContext context)
		{
			if (context.Target == null) {
				m_HasTarget = false;
				return;
			}

			m_SmoothedTarget = context.Target.position;
			m_HasTarget = true;
		}

		public void OnExit()
		{
		}

		public CameraPose Evaluate(float deltaTime, in CameraModeContext context)
		{
			if (context.Target == null) {
				return context.CurrentPose;
			}

			if (!m_HasTarget) {
				m_SmoothedTarget = context.Target.position;
				m_HasTarget = true;
			}

			float blend = 1.0f - Mathf.Exp(-m_PositionLerpSpeed * deltaTime);
			m_SmoothedTarget = Vector3.Lerp(m_SmoothedTarget, context.Target.position, blend);

			Vector3 desiredPosition = m_SmoothedTarget + m_WorldOffset;
			Vector3 lookPoint = context.Target.position + m_LookOffset;
			Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - desiredPosition, Vector3.up);

			return new CameraPose(desiredPosition, desiredRotation);
		}
	}
}
