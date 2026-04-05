using LitMotion;
using LitMotion.Extensions;
using TriInspector;
using UnityEngine;


namespace UI.Elements.Buttons.Animations.Rect
{
	[System.Serializable]
	public class ScaleTween : BaseTween
	{
		[SerializeField] private RectTransform m_TargetRect;
		[SerializeField] private Vector2       m_TargetScale;

		[SerializeField]                                  private bool    m_ResetOnPlay;
		[SerializeField, EnableIf(nameof(m_ResetOnPlay))] private Vector2 m_ResetScale;

		public override MotionHandle GetTween()
		{
			if (m_ResetOnPlay) {
				m_TargetRect.localScale = m_ResetScale;
			}

			return LMotion.Create((Vector2)m_TargetRect.localScale, m_TargetScale, Duration)
			              .WithEase(Ease)
			              .BindToLocalScaleXY(m_TargetRect);
		}
	}
}