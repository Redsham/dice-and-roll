using System;
using System.Collections.Generic;
using System.Linq;
using LitMotion;
using TriInspector;
using UnityEngine;


namespace UI.Elements.Buttons
{
	public class UIAnimatedButton : UISimpleButton
	{
		public interface IButtonTween
		{
			MotionHandle GetTween();
		}

		[Title("Tweens")]
		[SerializeReference] private IButtonTween[] m_HoverTweens;
		[SerializeReference] private IButtonTween[] m_UnhoverTweens;
		[SerializeReference] private IButtonTween[] m_PressedTweens;

		private readonly List<MotionHandle> m_Handles = new();


		private void OnEnable()
		{
			foreach (IButtonTween tween in m_UnhoverTweens) {
				tween.GetTween().Complete();
			}
		}

		protected override void OnHover()
		{
			base.OnHover();
			DoTweens(m_HoverTweens);
		}
		protected override void OnUnhover() => DoTweens(m_UnhoverTweens);
		protected override void OnPressed()
		{
			base.OnPressed();
			DoTweens(m_PressedTweens);
		}

		private void DoTweens(IButtonTween[] tweens)
		{
			foreach (MotionHandle tween in m_Handles.Where(tween => tween.IsPlaying())) {
				tween.Cancel();
			}

			m_Handles.Clear();

			foreach (IButtonTween tween in tweens.Where(x => x != null)) {
				m_Handles.Add(tween.GetTween());
			}
		}
	}
}