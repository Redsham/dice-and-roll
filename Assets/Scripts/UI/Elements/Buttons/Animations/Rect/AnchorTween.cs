using LitMotion;
using LitMotion.Extensions;
using TriInspector;
using UnityEngine;


namespace UI.Elements.Buttons.Animations.Rect
{
	public class AnchorTween : BaseTween
	{
		[SerializeField] private RectTransform m_TargetRect;
		[SerializeField] private Vector2       m_AnchorMin = Vector2.zero;
		[SerializeField] private Vector2       m_AnchorMax = Vector2.one;

		[SerializeField]                                  private bool    m_ResetOnPlay    = false;
		[SerializeField, EnableIf(nameof(m_ResetOnPlay))] private Vector2 m_ResetAnchorMin = Vector2.zero;
		[SerializeField, EnableIf(nameof(m_ResetOnPlay))] private Vector2 m_ResetAnchorMax = Vector2.one;

		public override MotionHandle GetTween()
		{
			if (m_ResetOnPlay) {
				m_TargetRect.anchorMin = m_ResetAnchorMin;
				m_TargetRect.anchorMax = m_ResetAnchorMax;
			}

			return LSequence.Create()
			                .Append(LMotion.Create(m_TargetRect.anchorMin, m_AnchorMin, Duration)
			                               .WithEase(Ease)
			                               .BindToAnchorMin(m_TargetRect))
			                .Join(LMotion.Create(m_TargetRect.anchorMax, m_AnchorMax, Duration)
			                             .WithEase(Ease)
			                             .BindToAnchorMax(m_TargetRect))
			                .Run();
		}
	}
}