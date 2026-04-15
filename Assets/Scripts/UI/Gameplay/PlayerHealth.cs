using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Gameplay
{
	public class PlayerHealth : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI m_Text;
		[SerializeField] private Image           m_Bar;

		public void SetHealth(int currentHealth, int maxHealth)
		{
			int clampedCurrentHealth = Mathf.Clamp(currentHealth, 0, Mathf.Max(0, maxHealth));

			if (m_Text != null) {
				m_Text.text = $"{clampedCurrentHealth}/{Mathf.Max(0, maxHealth)}";
			}

			if (m_Bar != null) {
				m_Bar.fillAmount = maxHealth > 0 ? Mathf.Clamp01((float)clampedCurrentHealth / maxHealth) : 0.0f;
			}
		}
	}
}
