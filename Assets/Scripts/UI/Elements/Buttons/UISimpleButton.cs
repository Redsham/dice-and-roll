using System;
using UI.Elements.Abstract;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI.Elements.Buttons
{
	public class UISimpleButton : UIButtonBase, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		public RectTransform Content => transform.GetChild(0) as RectTransform;
		public Image         Icon    => m_Icon;
		
		[SerializeField] private Image m_Icon;

		protected override void OnHover()   { }
		protected override void OnUnhover() { }

		protected override void OnPressed()  => OnClick.Invoke();
		protected override void OnReleased() { }

		public void OnPointerEnter(PointerEventData eventData) => OnHover();
		public void OnPointerExit(PointerEventData  eventData) => OnUnhover();

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				OnClick.Invoke();
		}
	}
}