using TriInspector;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Elements.Buttons
{
	public class UIRatioButton : UISimpleButton
	{
		public int Index { get; set; }

		[Title("Ratio Button")]
		[SerializeField] private Image m_Selected;

		public void OnSelect()
		{
			m_Selected.enabled = true;
		}

		public void OnDeselect()
		{
			m_Selected.enabled = false;
		}
	}
}