using System;
using R3;
using TriInspector;
using UI.Elements.Buttons;
using UnityEngine;


namespace UI.Elements
{
	public class UIRatioGroup : MonoBehaviour
	{
		/// Index of the currently selected button. -1 if no button is selected.
		public readonly ReactiveProperty<int> SelectedIndex = new(-1);

		[Title("Ratio Group")]
		[SerializeField] private int m_DefaultIndex = 0;
		[SerializeField] private bool m_Loop;
		
		private UIRatioButton[] m_Buttons = Array.Empty<UIRatioButton>();

		private void Start()
		{
			// Find all UIRatioButton components in children
			m_Buttons = GetComponentsInChildren<UIRatioButton>();
			if (m_Buttons.Length == 0) {
				Debug.LogError($"[{nameof(UIRatioGroup)}] No UIRatio buttons found");
				enabled = false;
				return;
			}

			// Set up button listeners
			for (int i = 0; i < m_Buttons.Length; i++) {
				int index = i;

				m_Buttons[i].Index = i;
				m_Buttons[i].OnClick.AddListener(() => Select(m_Buttons[index].Index));
				m_Buttons[i].OnDeselect();
			}

			// Select first element
			Select(m_DefaultIndex);
		}

		/// <summary>
		/// Select the button at the specified index
		/// </summary>
		/// <param name="index">Index of the button to select</param>
		public void Select(int index)
		{
			if (SelectedIndex.CurrentValue == index) return;
			if (index < 0 && index >= m_Buttons.Length) {
				Debug.LogError($"[{nameof(UIRatioGroup)}] Invalid index: {index}");
				return;
			}

			if (SelectedIndex.CurrentValue != -1)
				m_Buttons[SelectedIndex.CurrentValue].OnDeselect();

			SelectedIndex.Value = index;
			m_Buttons[index].OnSelect();
		}

		/// <summary>
		/// Select the next button in the group.
		/// If loop is enabled, it will wrap around to the first button when reaching the end of the list.
		/// </summary>
		public void Next()
		{
			if (SelectedIndex.CurrentValue == m_Buttons.Length - 1) {
				if (m_Loop) Select(0);
				else return;
			}
			
			Select(SelectedIndex.CurrentValue + 1);
		}
		/// <summary>
		/// Select the previous button in the group.
		/// If loop is enabled, it will wrap around to the last button when reaching the beginning of the list.
		/// </summary>
		public void Previous()
		{
			if (SelectedIndex.CurrentValue == 0) {
				if (m_Loop) Select(m_Buttons.Length - 1);
				else return;
			}
			
			Select(SelectedIndex.CurrentValue - 1);
		}
	}
}