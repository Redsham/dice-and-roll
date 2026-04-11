using System;
using LitMotion;
using LitMotion.Extensions;
using TriInspector;
using UnityEngine;


namespace UI.Elements.Buttons.Animations.Rect
{
	[Serializable]
	public class RotationTween : BaseTween
	{
		[SerializeField] private RectTransform m_TargetRect;
		[SerializeField] private float         m_TargetAngle;

		[SerializeField]                                  private bool  m_ResetOnPlay;
		[SerializeField, EnableIf(nameof(m_ResetOnPlay))] private float m_ResetAngle;

		public override MotionHandle GetTween()
		{
			if (m_ResetOnPlay) {
				m_TargetRect.eulerAngles = new(m_TargetRect.eulerAngles.x,
				                               m_TargetRect.eulerAngles.y,
				                               m_ResetAngle);
			}

			return LMotion.Create(m_TargetRect.eulerAngles.z, m_TargetAngle, Duration)
			              .WithEase(Ease)
			              .BindToEulerAnglesZ(m_TargetRect);
		}
	}
}