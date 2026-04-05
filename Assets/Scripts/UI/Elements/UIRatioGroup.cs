using System;
using R3;
using UI.Elements.Buttons;
using UnityEngine;


namespace UI.Elements
{
	public class UIRatioGroup : MonoBehaviour
	{
		// State

		public int               SelectedIndex { get; private set; } = -1;
		public event Action<int> OnSelected = delegate { };

		[SerializeField] private int m_DefaultIndex = 0;
		[SerializeField] private bool m_Loop;
		
		private UIRatioButton[] m_Buttons     = Array.Empty<UIRatioButton>();
		private bool            m_Initialized = false;

		// Lifecycle

		private void Awake()
		{
			EnsureInitialized();
		}

		// Selection

		/// <summary>
		/// Select the button at the specified index
		/// </summary>
		/// <param name="index">Index of the button to select</param>
		public void Select(int index, bool notify = true)
		{
			if (!EnsureInitialized()) {
				return;
			}

			if (SelectedIndex == index) return;
			if (index < 0 || index >= m_Buttons.Length) {
				Debug.LogError($"[{nameof(UIRatioGroup)}] Invalid index: {index}");
				return;
			}

			if (SelectedIndex != -1)
				m_Buttons[SelectedIndex].OnDeselect();

			SelectedIndex = index;
			m_Buttons[index].OnSelect();
			
			if(notify)
				OnSelected.Invoke(index);
		}

		/// <summary>
		/// Select the next button in the group.
		/// If loop is enabled, it will wrap around to the first button when reaching the end of the list.
		/// </summary>
		public void Next()
		{
			if (!EnsureInitialized()) {
				return;
			}

			if (SelectedIndex == m_Buttons.Length - 1) {
				if (m_Loop) Select(0);
				else return;
			}
			
			Select(SelectedIndex + 1);
		}
		/// <summary>
		/// Select the previous button in the group.
		/// If loop is enabled, it will wrap around to the last button when reaching the beginning of the list.
		/// </summary>
		public void Previous()
		{
			if (!EnsureInitialized()) {
				return;
			}

			if (SelectedIndex == 0) {
				if (m_Loop) Select(m_Buttons.Length - 1);
				else return;
			}
			
			Select(SelectedIndex - 1);
		}

		// Helpers

		private bool EnsureInitialized()
		{
			if (m_Initialized) {
				return m_Buttons.Length > 0;
			}

			m_Buttons = GetComponentsInChildren<UIRatioButton>(true);
			if (m_Buttons.Length == 0) {
				Debug.LogError($"[{nameof(UIRatioGroup)}] No UIRatio buttons found");
				enabled = false;
				return false;
			}

			for (int i = 0; i < m_Buttons.Length; i++) {
				int index = i;

				m_Buttons[i].Index = i;
				m_Buttons[i].OnClick.AddListener(() => Select(m_Buttons[index].Index));
				m_Buttons[i].OnDeselect();
			}

			m_Initialized = true;
			Select(Mathf.Clamp(m_DefaultIndex, 0, m_Buttons.Length - 1), false);
			return true;
		}
	}
}
