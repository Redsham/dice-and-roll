using Gameplay.Player.Runtime;
using LitMotion;
using R3;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


namespace UI.Gameplay
{
	public class PlayerHealthView : MonoBehaviour
	{
		// === Dependencies ===

		[Inject] private readonly DiceService m_Dice;

		// === Inspector ===

		[SerializeField] private TextMeshProUGUI m_Text;
		[SerializeField] private Image           m_Bar;
		[SerializeField] private Image           m_DeltaBar;

		[Title("Animations")]
		[SerializeField] private float m_DeltaAnimTime = 0.3f;
		[SerializeField] private float m_DeltaAnimDelay = 0.3f;
		[SerializeField] private Ease  m_DeltaEase      = Ease.OutExpo;

		// === State ===

		private MotionHandle m_DeltaHandle;



		[Inject]
		public void OnInjected()
		{
			m_Dice.CurrentHealth.Subscribe(health => SetHealth(health, m_Dice.MaxHealth)).AddTo(this);
		}

		private void SetHealth(int currentHealth, int maxHealth)
		{
			int clampedCurrentHealth = Mathf.Clamp(currentHealth, 0, Mathf.Max(0, maxHealth));

			if (m_Text != null) {
				m_Text.text = $"{clampedCurrentHealth}/{Mathf.Max(0, maxHealth)}";
			}

			if (m_Bar != null) {
				m_Bar.fillAmount = maxHealth > 0 ? Mathf.Clamp01((float)clampedCurrentHealth / maxHealth) : 0.0f;
			}

			if (m_DeltaBar != null) {
				m_DeltaHandle.TryCancel();
				
				m_Dice.CurrentHealth.Select(health => health > 0 && maxHealth > 0 ? Mathf.Clamp01((float)health / maxHealth) : 0.0f)
				      .DistinctUntilChanged()
				      .Subscribe(targetFill => {
					       m_DeltaHandle = LMotion.Create(m_DeltaBar.fillAmount, targetFill, m_DeltaAnimTime)
					                              .WithEase(m_DeltaEase)
					                              .WithDelay(m_DeltaAnimDelay)
					                              .Bind(fill => m_DeltaBar.fillAmount = fill);
				       })
				      .AddTo(this);
			}
		}
	}
}