using Gameplay.Enemies.Runtime;
using TMPro;
using UnityEngine;
using VContainer;


namespace UI.Scenes.Gameplay.Elements
{
	public class EnemiesCounterView : MonoBehaviour
	{
		// === Dependencies ===

		[Inject] private EnemyService m_EnemyService;

		// === Inspector ===

		[SerializeField] private TextMeshProUGUI m_CounterText;

		// === Lifecycle ===
		
		[Inject]
		public void OnInjected()
		{
			m_EnemyService.OnSpawned         += HandleEnemyChanged;
			m_EnemyService.OnDied            += HandleEnemyChanged;
			m_EnemyService.OnTrackingChanged += Refresh;
			Refresh();
		}
		private void OnDestroy()
		{
			if (m_EnemyService == null) {
				return;
			}

			m_EnemyService.OnSpawned         -= HandleEnemyChanged;
			m_EnemyService.OnDied            -= HandleEnemyChanged;
			m_EnemyService.OnTrackingChanged -= Refresh;
		}

		
		// === Updates ===
		
		private void UpdateText(int current, int? total)
		{
			if (m_CounterText == null) {
				return;
			}

			m_CounterText.text = total.HasValue
				                     ? $"{Mathf.Max(0, current)} / {Mathf.Max(0, total.Value)}"
				                     : Mathf.Max(0, current).ToString();
		}
		private void HandleEnemyChanged(EnemyRuntimeHandle _) => Refresh();
		private void Refresh()                                => UpdateText(m_EnemyService.AliveCount, m_EnemyService.PlannedEnemyCount);
	}
}