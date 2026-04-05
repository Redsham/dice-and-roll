using System;
using UnityEngine;


namespace UI.Elements.Other
{
	public class UIScreenSwitcher : MonoBehaviour
	{
		[Serializable]
		public class Screen
		{
			[field: SerializeField] public string     Name   { get; private set; }
			[field: SerializeField] public GameObject Object { get; private set; }
		}


		[SerializeField] private Screen[] m_Screens;
		private                  int      m_ActiveScreenIndex = -1;

		private void Start()
		{
			Switch(0);
		}

		public void Switch(int index)
		{
			if (m_ActiveScreenIndex == index) return;
			if (index < 0 || index >= m_Screens.Length) {
				Debug.LogError($"[{nameof(UIScreenSwitcher)}] Invalid screen index: {index}");
				return;
			}

			if (m_ActiveScreenIndex != -1)
				m_Screens[m_ActiveScreenIndex].Object.SetActive(false);

			m_ActiveScreenIndex = index;
			m_Screens[m_ActiveScreenIndex].Object.SetActive(true);
		}
		public void Switch(string screenName)
		{
			int index = Array.FindIndex(m_Screens, s => s.Name == screenName);
			if (index == -1) {
				Debug.LogError($"[{nameof(UIScreenSwitcher)}] No screen found with name: {screenName}");
				return;
			}

			Switch(index);
		}
	}
}