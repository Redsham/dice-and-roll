using Audio.UI;
using UI.Elements.Abstract;
using UnityEngine;


namespace UI.Effects
{
	[RequireComponent(typeof(UIButtonBase))]
	public class UIButtonSounds : MonoBehaviour
	{
		[SerializeField] private bool m_Click = true;
		[SerializeField] private bool m_Hover = true;

		private void Awake()
		{
			UIButtonBase button = GetComponent<UIButtonBase>();
			if (m_Click) button.OnClick.AddListener(() => UISounds.Play(UISoundsCue.Click));
			if (m_Hover) button.OnHovered.AddListener(() => UISounds.Play(UISoundsCue.Hover));
		}
	}
}