using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Runtime;
using TriInspector;
using UnityEngine;
using UnityEngine.Localization;


namespace UI.Tooltips
{
	[DisallowMultipleComponent]
	public sealed class EnemyTooltipTrigger : MonoBehaviour, ITooltipTrigger
	{
		[Title("References")]
		[SerializeField] private EnemyBehaviour m_Enemy;
		[SerializeField] private TooltipBase    m_TooltipPrefab;

		[Title("Presentation")]
		[SerializeField] private TooltipPresentationMode m_PresentationMode = TooltipPresentationMode.Callout;
		[SerializeField] private Vector2                 m_ScreenOffset     = new(28.0f, 20.0f);
		[SerializeField] private Transform               m_WorldAnchor;
		[SerializeField] private Vector3                 m_WorldOffset      = new(0.0f, 1.4f, 0.0f);

		public bool IsTooltipEnabled => isActiveAndEnabled && m_Enemy != null && m_TooltipPrefab != null;
		public TooltipBase TooltipPrefab => m_TooltipPrefab;
		public TooltipPresentationMode PresentationMode => m_PresentationMode;
		public TooltipAvailability Availability => TooltipAvailability.RequiresPlayerControl;
		public Vector2 ScreenOffset => m_ScreenOffset;

		private void Reset()
		{
			m_Enemy       = GetComponentInParent<EnemyBehaviour>();
			m_WorldAnchor = transform;
		}

		public bool TryGetWorldAnchor(out Vector3 worldAnchor)
		{
			Transform anchor = m_WorldAnchor != null ? m_WorldAnchor : transform;
			worldAnchor = anchor.position + m_WorldOffset;
			return true;
		}

		public void ConfigureTooltip(TooltipBase tooltip)
		{
			if (m_Enemy == null || tooltip is not EnemyTooltip enemyTooltip) {
				return;
			}

			EnemyConfig         config        = m_Enemy.Config;
			EnemyRuntimeHandle  runtimeHandle = m_Enemy.RuntimeHandle;
			int                 maxHealth     = config != null ? config.MaxHealth : 0;
			int                 currentHealth = runtimeHandle != null ? runtimeHandle.State.CurrentHealth : maxHealth;
			string              displayName   = ResolveLocalizedText(config != null ? config.DisplayName : null, m_Enemy.name);
			string              description   = ResolveLocalizedText(config != null ? config.Description : null, string.Empty);

			enemyTooltip.SetData(displayName, currentHealth, maxHealth, description);
		}

		private static string ResolveLocalizedText(LocalizedString localizedString, string fallback)
		{
			if (localizedString == null) {
				return fallback;
			}

			string resolved = localizedString.GetLocalizedString();
			return string.IsNullOrWhiteSpace(resolved) ? fallback : resolved;
		}
	}
}
