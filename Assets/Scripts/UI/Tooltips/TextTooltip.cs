using TMPro;
using UnityEngine;


namespace UI.Tooltips
{
	public sealed class TextTooltip : TooltipBase
	{
		[SerializeField] private TMP_Text m_Title;
		[SerializeField] private TMP_Text m_Description;

		public void SetText(string title, string description)
		{
			if (m_Title != null) {
				bool hasTitle = !string.IsNullOrWhiteSpace(title);
				m_Title.text = hasTitle ? title : string.Empty;
				m_Title.gameObject.SetActive(hasTitle);
			}

			if (m_Description != null) {
				bool hasDescription = !string.IsNullOrWhiteSpace(description);
				m_Description.text = hasDescription ? description : string.Empty;
				m_Description.gameObject.SetActive(hasDescription);
			}

			RefreshLayout();
		}
	}
}
