using TMPro;
using UnityEngine;


namespace UI.Tooltips
{
	public sealed class EnemyTooltip : TooltipBase
	{
		[SerializeField] private TextMeshProUGUI m_NameText;
		[SerializeField] private TextMeshProUGUI m_HealthText;
		[SerializeField] private TextMeshProUGUI m_DescriptionText;

		public void SetData(string enemyName, int currentHealth, int maxHealth, string description)
		{
			if (m_NameText != null) {
				m_NameText.text = enemyName;
			}

			if (m_HealthText != null) {
				m_HealthText.text = $"{Mathf.Max(0, currentHealth)}/{Mathf.Max(0, maxHealth)}";
			}

			if (m_DescriptionText != null) {
				m_DescriptionText.text = description;
			}

			RefreshLayout();
		}
	}
}
