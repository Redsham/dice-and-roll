using UnityEngine;
using UnityEngine.Events;


namespace UI.Elements.Abstract
{
	public abstract class UIButtonBase : MonoBehaviour
	{
		/// <summary>
		///     Button click event. This event is invoked when the button is clicked.
		/// </summary>
		public UnityEvent OnClick;

		/// Button hovered event. This event is invoked when the button is hovered.
		public UnityEvent OnHovered;

		/// Button unhovered event. This event is invoked when the button is unhovered.
		public UnityEvent OnUnhovered;

		// Hover
		protected abstract void OnHover();
		protected abstract void OnUnhover();

		// Pressed
		protected abstract void OnPressed();
		protected abstract void OnReleased();
	}
}