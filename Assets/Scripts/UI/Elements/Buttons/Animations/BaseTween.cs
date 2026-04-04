using LitMotion;
using UnityEngine;


namespace UI.Elements.Buttons.Animations
{
	public abstract class BaseTween : UIAnimatedButton.IButtonTween
	{
		[SerializeField] protected float Duration;
		[SerializeField] protected Ease  Ease = Ease.Linear;
		
		public abstract MotionHandle GetTween();
	}
}