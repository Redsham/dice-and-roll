using TriInspector;
using UnityEngine;


namespace UI.Tooltips
{
	[DisallowMultipleComponent]
	public sealed class TooltipTrigger : MonoBehaviour, ITooltipTrigger
	{
		[Title("Tooltip")]
		[SerializeField] private TooltipBase             m_TooltipPrefab;
		[SerializeField] private TooltipPresentationMode m_PresentationMode = TooltipPresentationMode.Automatic;
		[SerializeField] private Vector2                 m_ScreenOffset     = new(20.0f, 20.0f);

		[Title("Content")]
		[SerializeField] private string m_Title;
		[SerializeField] private string m_Description;

		[Title("World Callout")]
		[SerializeField] private Transform m_WorldAnchor;
		[SerializeField] private Vector3   m_WorldOffset = Vector3.up;

		public bool IsTooltipEnabled => isActiveAndEnabled && m_TooltipPrefab != null;
		public TooltipBase TooltipPrefab => m_TooltipPrefab;
		public TooltipPresentationMode PresentationMode => m_PresentationMode;
		public Vector2 ScreenOffset => m_ScreenOffset;

		public bool TryGetWorldAnchor(out Vector3 worldAnchor)
		{
			Transform anchor = m_WorldAnchor != null ? m_WorldAnchor : transform;
			worldAnchor = anchor.position + m_WorldOffset;

			return m_PresentationMode == TooltipPresentationMode.Callout || m_WorldAnchor != null;
		}

		public void ConfigureTooltip(TooltipBase tooltip)
		{
			if (tooltip is TextTooltip textTooltip) {
				textTooltip.SetText(m_Title, m_Description);
			}
		}
	}
}
