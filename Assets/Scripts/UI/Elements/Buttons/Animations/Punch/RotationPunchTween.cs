using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;


namespace UI.Elements.Buttons.Animations.Punch
{
	[Serializable]
	public class RotationPunchTween : PunchTween
	{
		[SerializeField] private float m_StartAngle;
		[SerializeField] private float m_PunchScale = 30.0f;

		public override MotionHandle GetTween()
		{
			return LMotion.Punch.Create(m_StartAngle, m_PunchScale, Duration)
			              .WithFrequency(Frequency)
			              .WithDampingRatio(DampingRatio)
			              .WithEase(Ease)
			              .BindToEulerAnglesZ(TargetRect);
		}
	}
}