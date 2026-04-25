using UnityEngine;


namespace UI.Tooltips
{
	public interface ITooltipTrigger
	{
		bool IsTooltipEnabled { get; }
		TooltipBase TooltipPrefab { get; }
		TooltipPresentationMode PresentationMode { get; }
		Vector2 ScreenOffset { get; }

		bool TryGetWorldAnchor(out Vector3 worldAnchor);
		void ConfigureTooltip(TooltipBase tooltip);
	}
}
