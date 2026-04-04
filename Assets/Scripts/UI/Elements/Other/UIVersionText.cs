using TMPro;
using UnityEngine;


namespace UI.Elements.Other
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class UIVersionText : MonoBehaviour
	{
		private void Awake()
		{
			TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
			text.text = $"Version: {Application.version}";
		}
	}
}