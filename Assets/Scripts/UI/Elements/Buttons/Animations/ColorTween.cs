using LitMotion;
using LitMotion.Extensions;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Elements.Buttons.Animations
{
	public class ColorTween : BaseTween
	{
		[SerializeField] private Graphic m_TargetGraphic;
		[SerializeField] private Color    m_TargetColor;

		[SerializeField] private bool m_ResetOnPlay = false;
		[SerializeField, EnableIf(nameof(m_ResetOnPlay))] private Color m_ResetColor = Color.white;

		public override MotionHandle GetTween()
		{
			if (m_ResetOnPlay) {
				m_TargetGraphic.color = m_ResetColor;
			}
			
			return LMotion.Create(m_TargetGraphic.color, m_TargetColor, Duration)
			              .WithEase(Ease)
			              .BindToColor(m_TargetGraphic);
		}
	}
}