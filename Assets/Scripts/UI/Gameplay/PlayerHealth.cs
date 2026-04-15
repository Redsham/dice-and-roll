using Gameplay.Player.Runtime;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


namespace UI.Gameplay
{
	public class PlayerHealth : MonoBehaviour
	{
		[Inject] private readonly DiceService m_Dice;
		
		[SerializeField] private TextMeshProUGUI m_Text;
		[SerializeField] private Image           m_Bar;
		
		[Inject] public void OnInjected()
		{
			m_Dice.CurrentHealth.Subscribe(health => SetHealth(health, m_Dice.MaxHealth)).AddTo(this);
		}

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
