using UnityEngine;


namespace UI.Elements.Buttons.Animations.Punch
{
	[System.Serializable]
	public abstract class PunchTween : BaseTween
	{
		[SerializeField] protected RectTransform TargetRect;
		[SerializeField] protected int           Frequency = 2;
		[SerializeField] protected float         DampingRatio   = 5.0f;
	}
}