using Audio.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI.Effects
{
	[RequireComponent(typeof(Toggle))]
	public class UIDropdownItemSounds : MonoBehaviour, IPointerEnterHandler
	{
		[SerializeField] private bool m_Hover = true;

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (m_Hover) UISounds.Play(UISoundsCue.Hover);
		}
	}
}
