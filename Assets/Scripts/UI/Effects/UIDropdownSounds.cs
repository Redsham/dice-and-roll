using Audio.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


namespace UI.Effects
{
	[RequireComponent(typeof(TMP_Dropdown))]
	public class UIDropdownSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
	{
		[SerializeField] private bool m_Click = true;
		[SerializeField] private bool m_Hover = true;
		[SerializeField] private bool m_ValueChanged = true;

		private TMP_Dropdown m_Dropdown;

		private void Awake()
		{
			m_Dropdown = GetComponent<TMP_Dropdown>();
			if (m_ValueChanged) m_Dropdown.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnDestroy()
		{
			if (m_Dropdown != null) {
				m_Dropdown.onValueChanged.RemoveListener(OnValueChanged);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (m_Hover) UISounds.Play(UISoundsCue.Hover);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (m_Click) UISounds.Play(UISoundsCue.Click);
		}

		private void OnValueChanged(int value)
		{
			UISounds.Play(UISoundsCue.Click);
		}
	}
}
