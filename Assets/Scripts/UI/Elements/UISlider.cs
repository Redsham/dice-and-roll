using R3;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Elements
{
	public class UISlider : MonoBehaviour
	{
		public readonly ReactiveProperty<float> Value = new();

		[SerializeField, Required] private Slider          m_Slider;
		[SerializeField]           private TextMeshProUGUI m_Text;

		
		private void Start()
		{
			if (m_Slider == null) {
				Debug.LogError($"[{nameof(UISlider)}] Slider component is not assigned.", this);
				enabled = false;
				return;
			}

			m_Slider.onValueChanged.AddListener(OnSliderValueChanged);

			if (m_Text != null) {
				Value.Subscribe(value => {
					m_Text.text = Mathf.RoundToInt(value * 100).ToString();
				}).AddTo(this);
			}
		}

		private void OnSliderValueChanged(float value)
		{
			Value.Value = value;
		}

		public void SetValue(float value)
		{
			Value.Value = value;
			m_Slider.SetValueWithoutNotify(value);
		}
	}
}