using System;
using TriInspector;
using UI.Elements.Abstract;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI.Elements.Buttons
{
	public class UISimpleButton : UIButtonBase, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		public RectTransform Content       => transform.GetChild(0) as RectTransform;
		public Image         IconComponent => m_IconComponent;

		[Title("Icon")]
		[SerializeField, DisableIf(nameof(m_IconComponent), null)] private Sprite m_Icon;
		[SerializeField] private Image m_IconComponent;


		private void OnValidate()
		{
			if (m_IconComponent != null) {
				m_IconComponent.sprite = m_Icon;
				m_IconComponent.gameObject.SetActive(m_Icon != null);
			}
		}

		protected override void OnHover()   { }
		protected override void OnUnhover() { }

		protected override void OnPressed()  => OnClick.Invoke();
		protected override void OnReleased() { }

		public void OnPointerEnter(PointerEventData eventData) => OnHover();
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				OnPressed();
		}
		public void OnPointerExit(PointerEventData eventData) => OnUnhover();
	}
}