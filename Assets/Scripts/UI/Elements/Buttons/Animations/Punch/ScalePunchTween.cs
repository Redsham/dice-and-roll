using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;


namespace UI.Elements.Buttons.Animations.Punch
{
	[Serializable]
	public class ScalePunchTween : PunchTween
	{
		[SerializeField] private Vector2 m_PunchScale;

		public override MotionHandle GetTween()
		{
			return LMotion.Punch.Create(Vector2.one, m_PunchScale, Duration)
			              .WithFrequency(Frequency)
			              .WithDampingRatio(DampingRatio)
			              .WithEase(Ease)
			              .BindToLocalScaleXY(TargetRect);
		}
	}
}