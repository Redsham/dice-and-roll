using System.Reflection;
using UI.Elements.Other;
using UnityEditor;
using UnityEngine;


namespace UI.Editor
{
	[InitializeOnLoad]
	public static class UIScreenSwitcherSelectionHandler
	{
		static UIScreenSwitcherSelectionHandler()
		{
			Selection.selectionChanged += OnSelectionChanged;
		}

		private static void OnSelectionChanged()
		{
			GameObject selected = Selection.activeGameObject;
			if (selected == null)
				return;

			UIScreenSwitcher switcher = selected.GetComponentInParent<UIScreenSwitcher>(true);
			if (switcher == null)
				return;

			FieldInfo screensField = typeof(UIScreenSwitcher)
			   .GetField("m_Screens", BindingFlags.NonPublic | BindingFlags.Instance);

			UIScreenSwitcher.Screen[] screens = screensField?.GetValue(switcher) as UIScreenSwitcher.Screen[];
			if (screens == null)
				return;

			for (int i = 0; i < screens.Length; i++)
			{
				if (screens[i].Object == selected)
				{
					switcher.Switch(i);

					EditorUtility.SetDirty(switcher);
					break;
				}
			}
		}
	}
}