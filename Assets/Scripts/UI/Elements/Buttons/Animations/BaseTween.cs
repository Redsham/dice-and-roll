using LitMotion;
using UnityEngine;


namespace UI.Elements.Buttons.Animations
{
	[System.Serializable]
	public abstract class BaseTween : UIAnimatedButton.IButtonTween
	{
		[SerializeField] protected float          Duration       = 0.25f;
		[SerializeField] protected Ease           Ease           = Ease.Linear;

		public abstract MotionHandle GetTween();
	}
}