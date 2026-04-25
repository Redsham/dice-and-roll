using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
using UnityEngine;


namespace Gameplay.Camera.Modes
{
	public sealed class FocusWorldPointCameraMode : ICameraMode
	{
		private readonly Vector3 m_WorldPoint;
		private readonly FocusWorldPointCameraSettings m_Settings;

		private Vector3 m_SmoothedTarget;
		private Vector3 m_FallbackDirection;
		private float   m_PlanarDistance;
		private float   m_HeightOffset;
		private float   m_SideSign;
		private bool    m_HasTarget;
		private Vector3 m_SmoothedLookPoint;

		public FocusWorldPointCameraMode(
			Vector3 worldPoint,
			FocusWorldPointCameraSettings settings
		)
		{
			m_WorldPoint = worldPoint;
			m_Settings   = settings;
		}

		public void OnEnter(in CameraModeContext context)
		{
			if (context.Target == null) {
				m_HasTarget = false;
				return;
			}

			Vector3 targetPosition = context.Target.position;
			Vector3 targetToCamera = context.CurrentPose.Position - targetPosition;
			Vector3 planarOffset   = Vector3.ProjectOnPlane(targetToCamera, Vector3.up);

			m_PlanarDistance = planarOffset.magnitude;
			if (m_PlanarDistance <= 0.01f) {
				m_PlanarDistance = 7.0f;
			}

			m_FallbackDirection = planarOffset.sqrMagnitude > 0.0001f ? planarOffset.normalized : Vector3.back;
			Vector3 right = Vector3.Cross(Vector3.up, m_FallbackDirection);
			m_SideSign = Vector3.Dot(planarOffset, right) >= 0.0f ? 1.0f : -1.0f;
			m_HeightOffset   = targetToCamera.y + m_Settings.HeightOffset;
			m_PlanarDistance *= m_Settings.DistanceMultiplier;
			m_SmoothedTarget    = targetPosition;
			m_SmoothedLookPoint = targetPosition + Vector3.up;
			m_HasTarget         = true;
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
				OnEnter(context);
			}

			float blend = 1.0f - Mathf.Exp(-m_Settings.PositionLerpSpeed * deltaTime);
			m_SmoothedTarget = Vector3.Lerp(m_SmoothedTarget, context.Target.position, blend);

			Vector3 focusDirection = Vector3.ProjectOnPlane(m_WorldPoint - m_SmoothedTarget, Vector3.up);
			Vector3 forward = focusDirection.sqrMagnitude > 0.0001f ? focusDirection.normalized : -m_FallbackDirection;
			Vector3 right   = Vector3.Cross(Vector3.up, forward);

			Vector3 planarOffset = (-forward * m_Settings.BackOffset) + (right * m_Settings.SideOffset * m_SideSign);
			if (planarOffset.sqrMagnitude <= 0.0001f) {
				planarOffset = m_FallbackDirection;
			}

			Vector3 desiredPosition = m_SmoothedTarget + planarOffset.normalized * m_PlanarDistance + Vector3.up * m_HeightOffset;

			Vector3 targetLookPoint = m_SmoothedTarget + Vector3.up * 1.0f;
			Vector3 eventLookPoint  = new(m_WorldPoint.x, targetLookPoint.y, m_WorldPoint.z);
			Vector3 focusPoint      = Vector3.Lerp(m_SmoothedTarget, m_WorldPoint, m_Settings.FocusPointBlend);
			Vector3 cinematicLookPoint = Vector3.Lerp(focusPoint + Vector3.up * 1.0f, eventLookPoint, m_Settings.EventLookBlend);
			Vector3 desiredLookPoint = Vector3.Lerp(targetLookPoint, cinematicLookPoint, 0.85f);
			float lookBlend = 1.0f - Mathf.Exp(-m_Settings.LookLerpSpeed * deltaTime);
			m_SmoothedLookPoint = Vector3.Lerp(m_SmoothedLookPoint, desiredLookPoint, lookBlend);
			Quaternion desiredRotation = Quaternion.LookRotation(m_SmoothedLookPoint - desiredPosition, Vector3.up);

			return new(desiredPosition, desiredRotation);
		}
	}

	public sealed class FocusTrackedTransformCameraMode : ICameraMode
	{
		private readonly Transform m_TrackedTransform;
		private readonly FocusTrackedTransformCameraSettings m_Settings;

		private Vector3 m_SmoothedAnchor;
		private Vector3 m_SmoothedTracked;
		private Vector3 m_TrackedStartPosition;
		private Vector3 m_FallbackDirection;
		private float   m_PlanarDistance;
		private float   m_HeightOffset;
		private float   m_SideSign;
		private bool    m_HasTarget;
		private Vector3 m_SmoothedLookPoint;

		public FocusTrackedTransformCameraMode(Transform trackedTransform, FocusTrackedTransformCameraSettings settings)
		{
			m_TrackedTransform = trackedTransform;
			m_Settings         = settings;
		}

		public void OnEnter(in CameraModeContext context)
		{
			if (context.Target == null) {
				m_HasTarget = false;
				return;
			}

			Vector3 anchorPosition  = context.Target.position;
			Vector3 trackedPosition = m_TrackedTransform != null ? m_TrackedTransform.position : anchorPosition;
			Vector3 targetToCamera  = context.CurrentPose.Position - anchorPosition;
			Vector3 planarOffset    = Vector3.ProjectOnPlane(targetToCamera, Vector3.up);

			m_PlanarDistance = planarOffset.magnitude;
			if (m_PlanarDistance <= 0.01f) {
				m_PlanarDistance = 7.0f;
			}

			m_PlanarDistance *= m_Settings.DistanceMultiplier;
			m_FallbackDirection = planarOffset.sqrMagnitude > 0.0001f ? planarOffset.normalized : Vector3.back;
			Vector3 right = Vector3.Cross(Vector3.up, m_FallbackDirection);
			m_SideSign = Vector3.Dot(planarOffset, right) >= 0.0f ? 1.0f : -1.0f;
			m_HeightOffset    = m_Settings.HeightOffset;
			m_SmoothedAnchor  = anchorPosition;
			m_SmoothedTracked = trackedPosition;
			m_SmoothedLookPoint = trackedPosition + Vector3.up;
			m_TrackedStartPosition = trackedPosition;
			m_HasTarget       = true;
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
				OnEnter(context);
			}

			float blend = 1.0f - Mathf.Exp(-m_Settings.PositionLerpSpeed * deltaTime);
			m_SmoothedAnchor = Vector3.Lerp(m_SmoothedAnchor, context.Target.position, blend);

			Vector3 trackedPosition = m_TrackedTransform != null ? m_TrackedTransform.position : m_SmoothedTracked;
			m_SmoothedTracked = Vector3.Lerp(m_SmoothedTracked, trackedPosition, blend);

			Vector3 anchorToTracked = Vector3.ProjectOnPlane(m_SmoothedTracked - m_SmoothedAnchor, Vector3.up);
			Vector3 forward = anchorToTracked.sqrMagnitude > 0.0001f ? anchorToTracked.normalized : -m_FallbackDirection;
			Vector3 right   = Vector3.Cross(Vector3.up, forward);

			Vector3 planarOffset = (-forward * m_Settings.BackOffset) + (right * m_Settings.SideOffset * m_SideSign);
			if (planarOffset.sqrMagnitude <= 0.0001f) {
				planarOffset = m_FallbackDirection;
			}

			Vector3 trackedDisplacement = m_SmoothedTracked - m_TrackedStartPosition;
			Vector3 parallaxOffset = Vector3.ProjectOnPlane(trackedDisplacement, Vector3.up) * m_Settings.PlanarParallax
			                       + Vector3.up * (trackedDisplacement.y * m_Settings.VerticalParallax);

			Vector3 desiredPosition = m_SmoothedAnchor + planarOffset.normalized * m_PlanarDistance + Vector3.up * m_HeightOffset + parallaxOffset;

			Vector3 anchorLookPoint  = m_SmoothedAnchor + Vector3.up;
			Vector3 trackedLookPoint = m_SmoothedTracked + Vector3.up;
			Vector3 compositionPoint = Vector3.Lerp(m_SmoothedAnchor, m_SmoothedTracked, m_Settings.AnchorLookBlend);
			Vector3 desiredLookPoint = Vector3.Lerp(compositionPoint + Vector3.up, trackedLookPoint, m_Settings.TrackedLookBlend);
			desiredLookPoint = Vector3.Lerp(anchorLookPoint, desiredLookPoint, 0.9f);
			float lookBlend = 1.0f - Mathf.Exp(-m_Settings.LookLerpSpeed * deltaTime);
			m_SmoothedLookPoint = Vector3.Lerp(m_SmoothedLookPoint, desiredLookPoint, lookBlend);

			Quaternion desiredRotation = Quaternion.LookRotation(m_SmoothedLookPoint - desiredPosition, Vector3.up);
			return new(desiredPosition, desiredRotation);
		}
	}
}
