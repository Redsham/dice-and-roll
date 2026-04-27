using Audio.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI.Effects
{
	[RequireComponent(typeof(Slider))]
	public class UISliderSounds : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
	{
		[SerializeField] private bool  m_Click = true;
		[SerializeField] private bool  m_Hover = true;
		[SerializeField] private bool  m_Scroll = true;
		[SerializeField] private float m_ScrollCooldown = 0.05f;

		private Slider m_Slider;
		private float  m_LastScrollTime = float.NegativeInfinity;

		private void Awake()
		{
			m_Slider = GetComponent<Slider>();
			if (m_Scroll) m_Slider.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnDestroy()
		{
			if (m_Slider != null) {
				m_Slider.onValueChanged.RemoveListener(OnValueChanged);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (m_Hover) UISounds.Play(UISoundsCue.Hover);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (m_Click) UISounds.Play(UISoundsCue.Click);
		}

		private void OnValueChanged(float value)
		{
			if (Time.unscaledTime < m_LastScrollTime + m_ScrollCooldown) {
				return;
			}

			m_LastScrollTime = Time.unscaledTime;
			UISounds.Play(UISoundsCue.Scroll);
		}
	}
}
