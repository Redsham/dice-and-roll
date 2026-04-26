using System;
using System.Collections.Generic;
using Infrastructure.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;


namespace UI.Tooltips
{
	public sealed class TooltipService : IStartable, ITickable, IDisposable
	{
		private const int TOOLTIP_SORTING_ORDER = 2500;

		[Inject] private readonly IObjectResolver            m_Resolver;
		[Inject] private readonly PlayerControlStateService  m_PlayerControlStateService;

		private readonly List<RaycastResult> m_RaycastResults = new();
		private readonly RaycastHit[]        m_RaycastHits    = new RaycastHit[16];

		private Canvas         m_Canvas;
		private RectTransform  m_CanvasRect;
		private TooltipBase    m_CurrentTooltip;
		private ITooltipTrigger m_CurrentTrigger;
		private Component      m_CurrentTriggerComponent;
		private int            m_CurrentTooltipVersion;

		public void Start()
		{
			GameObject canvasRoot = new("TooltipCanvas");
			Object.DontDestroyOnLoad(canvasRoot);
			
			m_Canvas = canvasRoot.AddComponent<Canvas>();
			m_Canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
			m_Canvas.sortingOrder = TOOLTIP_SORTING_ORDER;

			canvasRoot.AddComponent<CanvasScaler>();
			canvasRoot.AddComponent<GraphicRaycaster>();

			m_CanvasRect = (RectTransform)m_Canvas.transform;
		}

		public void Tick()
		{
			if (!TryGetPointerPosition(out Vector2 pointerPosition)) {
				HideCurrentTooltip();
				return;
			}

			if (!TryResolveHoveredTrigger(pointerPosition, out HoveredTrigger hoveredTrigger)) {
				HideCurrentTooltip();
				return;
			}

			if (!IsTriggerAvailable(hoveredTrigger.Trigger)) {
				HideCurrentTooltip();
				return;
			}

			if (!ReferenceEquals(m_CurrentTrigger, hoveredTrigger.Trigger) || !ReferenceEquals(m_CurrentTriggerComponent, hoveredTrigger.TriggerComponent)) {
				ShowTooltip(hoveredTrigger);
			}
			else {
				RefreshCurrentTooltip(hoveredTrigger);
			}

			UpdateTooltipPlacement(hoveredTrigger);
		}

		public void Dispose()
		{
			m_CurrentTooltipVersion++;

			if (m_CurrentTooltip != null) {
				Object.Destroy(m_CurrentTooltip.gameObject);
			}

			if (m_Canvas != null) {
				Object.Destroy(m_Canvas.gameObject);
			}
		}

		private void ShowTooltip(HoveredTrigger hoveredTrigger)
		{
			m_CurrentTooltipVersion++;
			HideCurrentTooltip();

			TooltipBase tooltipPrefab = hoveredTrigger.Trigger.TooltipPrefab;
			if (tooltipPrefab == null) {
				return;
			}

			m_CurrentTrigger          = hoveredTrigger.Trigger;
			m_CurrentTriggerComponent = hoveredTrigger.TriggerComponent;
			m_CurrentTooltip          = m_Resolver.Instantiate(tooltipPrefab, m_CanvasRect);

			RectTransform rootRect = (RectTransform)m_CurrentTooltip.transform;
			rootRect.anchorMin        = Vector2.zero;
			rootRect.anchorMax        = Vector2.one;
			rootRect.offsetMin        = Vector2.zero;
			rootRect.offsetMax        = Vector2.zero;
			rootRect.anchoredPosition = Vector2.zero;
			rootRect.localScale       = Vector3.one;

			RefreshCurrentTooltip(hoveredTrigger);
			m_CurrentTooltip.PlayShow();
		}

		private void RefreshCurrentTooltip(HoveredTrigger hoveredTrigger)
		{
			if (m_CurrentTooltip == null) {
				return;
			}

			hoveredTrigger.Trigger.ConfigureTooltip(m_CurrentTooltip);
			m_CurrentTooltip.RefreshLayout();
		}

		private void HideCurrentTooltip()
		{
			if (m_CurrentTooltip == null) {
				m_CurrentTrigger          = null;
				m_CurrentTriggerComponent = null;
				return;
			}

			int         version = ++m_CurrentTooltipVersion;
			TooltipBase tooltip = m_CurrentTooltip;

			m_CurrentTooltip          = null;
			m_CurrentTrigger          = null;
			m_CurrentTriggerComponent = null;

			tooltip.PlayHide(() => {
				if (version == m_CurrentTooltipVersion && tooltip != null) {
					Object.Destroy(tooltip.gameObject);
					return;
				}

				if (tooltip != null) {
					Object.Destroy(tooltip.gameObject);
				}
			});
		}

		private void UpdateTooltipPlacement(HoveredTrigger hoveredTrigger)
		{
			if (m_CurrentTooltip == null) {
				return;
			}

			Vector2 screenAnchor = hoveredTrigger.ScreenAnchor;
			if (hoveredTrigger.HasWorldAnchor) {
				if (!TryProjectWorldToScreenPoint(hoveredTrigger.WorldAnchor, out screenAnchor, out bool isBehindCamera) || isBehindCamera) {
					HideCurrentTooltip();
					return;
				}
			}

			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, screenAnchor, null, out Vector2 canvasAnchor)) {
				HideCurrentTooltip();
				return;
			}

			m_CurrentTooltip.RefreshLayout();

			RectTransform panelRect     = m_CurrentTooltip.PanelRect;
			Vector2       tooltipSize   = panelRect.rect.size;
			Vector2       screenPadding = new(18.0f, 18.0f);
			Vector2       offset        = hoveredTrigger.Trigger.ScreenOffset;
			Vector2       pivotSeed     = new(screenAnchor.x < Screen.width * 0.5f ? 0.0f : 1.0f, screenAnchor.y < Screen.height * 0.5f ? 0.0f : 1.0f);
			Vector2[]     pivots        = GetPivotCandidates(pivotSeed);

			float   bestOverflow = float.MaxValue;
			Vector2 bestPivot    = pivots[0];
			Vector2 bestPosition = canvasAnchor;

			for (int i = 0; i < pivots.Length; i++) {
				Vector2 pivot    = pivots[i];
				Vector2 position = GetTooltipPosition(canvasAnchor, offset, pivot);
				float   overflow = CalculateOverflow(position, pivot, tooltipSize, screenPadding);

				if (overflow < bestOverflow) {
					bestOverflow = overflow;
					bestPivot    = pivot;
					bestPosition = position;
				}
			}

			bestPosition = ClampPosition(bestPosition, bestPivot, tooltipSize, screenPadding);
			m_CurrentTooltip.SetPlacement(bestPosition, bestPivot);

			bool showCallout = ResolvePresentationMode(hoveredTrigger) == TooltipPresentationMode.Callout;
			m_CurrentTooltip.SetCallout(canvasAnchor, showCallout);
		}

		private bool TryResolveHoveredTrigger(Vector2 pointerPosition, out HoveredTrigger hoveredTrigger)
		{
			if (TryResolveUiTrigger(pointerPosition, out hoveredTrigger)) {
				return true;
			}

			return TryResolveWorldTrigger(pointerPosition, out hoveredTrigger);
		}

		private bool TryResolveUiTrigger(Vector2 pointerPosition, out HoveredTrigger hoveredTrigger)
		{
			hoveredTrigger = default;
			if (EventSystem.current == null) {
				return false;
			}

			m_RaycastResults.Clear();

			PointerEventData pointerEventData = new(EventSystem.current) {
				position = pointerPosition
			};

			EventSystem.current.RaycastAll(pointerEventData, m_RaycastResults);
			for (int i = 0; i < m_RaycastResults.Count; i++) {
				RaycastResult raycastResult = m_RaycastResults[i];
				if (!TryGetTooltipTrigger(raycastResult.gameObject, out ITooltipTrigger trigger, out Component triggerComponent)) {
					continue;
				}

				hoveredTrigger = new(trigger, triggerComponent, pointerPosition, false, default);
				return true;
			}

			return false;
		}

		private bool TryResolveWorldTrigger(Vector2 pointerPosition, out HoveredTrigger hoveredTrigger)
		{
			hoveredTrigger = default;
			if (!TryCreateScreenRay(pointerPosition, out Ray ray)) {
				return false;
			}

			int hitCount = Physics.RaycastNonAlloc(ray, m_RaycastHits, float.MaxValue, ~0, QueryTriggerInteraction.Collide);
			float nearestDistance = float.MaxValue;

			for (int i = 0; i < hitCount; i++) {
				RaycastHit hit = m_RaycastHits[i];
				if (hit.collider == null || hit.distance >= nearestDistance) {
					continue;
				}

				if (!TryGetTooltipTrigger(hit.collider.gameObject, out ITooltipTrigger trigger, out Component triggerComponent)) {
					continue;
				}

				Vector3 worldAnchor = hit.point;
				if (trigger.TryGetWorldAnchor(out Vector3 configuredAnchor)) {
					worldAnchor = configuredAnchor;
				}

				nearestDistance = hit.distance;
				hoveredTrigger  = new(trigger, triggerComponent, pointerPosition, true, worldAnchor);
			}

			return hoveredTrigger.Trigger != null;
		}

		private static bool TryGetPointerPosition(out Vector2 pointerPosition)
		{
			if (Pointer.current != null) {
				pointerPosition = Pointer.current.position.ReadValue();
				return true;
			}

			if (Mouse.current != null) {
				pointerPosition = Mouse.current.position.ReadValue();
				return true;
			}

			pointerPosition = default;
			return false;
		}

		private bool IsTriggerAvailable(ITooltipTrigger trigger)
		{
			return trigger.Availability switch {
				TooltipAvailability.Always                => true,
				TooltipAvailability.RequiresPlayerControl => m_PlayerControlStateService.HasControl,
				_                                         => true
			};
		}

		private static bool TryCreateScreenRay(Vector2 screenPosition, out Ray ray)
		{
			Camera camera = GetCurrentCamera();
			if (camera == null) {
				ray = default;
				return false;
			}

			ray = camera.ScreenPointToRay(screenPosition);
			return true;
		}

		private static bool TryProjectWorldToScreenPoint(Vector3 worldPoint, out Vector2 screenPoint, out bool isBehindCamera)
		{
			Camera camera = GetCurrentCamera();
			if (camera == null) {
				screenPoint    = default;
				isBehindCamera = true;
				return false;
			}

			Vector3 projectedPoint = camera.WorldToScreenPoint(worldPoint);
			screenPoint    = projectedPoint;
			isBehindCamera = projectedPoint.z < 0.0f;
			return !isBehindCamera;
		}

		private static Camera GetCurrentCamera()
		{
			if (Camera.main != null) {
				return Camera.main;
			}

			return Object.FindFirstObjectByType<Camera>();
		}

		private static TooltipPresentationMode ResolvePresentationMode(HoveredTrigger hoveredTrigger)
		{
			return hoveredTrigger.Trigger.PresentationMode switch {
				TooltipPresentationMode.Automatic => hoveredTrigger.HasWorldAnchor ? TooltipPresentationMode.Callout : TooltipPresentationMode.Inline,
				_                                 => hoveredTrigger.Trigger.PresentationMode
			};
		}

		private static bool TryGetTooltipTrigger(GameObject gameObject, out ITooltipTrigger trigger, out Component triggerComponent)
		{
			MonoBehaviour[] behaviours = gameObject.GetComponentsInParent<MonoBehaviour>(includeInactive: false);
			for (int i = 0; i < behaviours.Length; i++) {
				if (behaviours[i] is not ITooltipTrigger tooltipTrigger || !tooltipTrigger.IsTooltipEnabled) {
					continue;
				}

				trigger          = tooltipTrigger;
				triggerComponent = behaviours[i];
				return true;
			}

			trigger          = null;
			triggerComponent = null;
			return false;
		}

		private static Vector2[] GetPivotCandidates(Vector2 primaryPivot)
		{
			Vector2 oppositeX = new(primaryPivot.x > 0.5f ? 0.0f : 1.0f, primaryPivot.y);
			Vector2 oppositeY = new(primaryPivot.x, primaryPivot.y > 0.5f ? 0.0f : 1.0f);
			Vector2 opposite  = new(oppositeX.x, oppositeY.y);

			return new[] { primaryPivot, oppositeX, oppositeY, opposite };
		}

		private static Vector2 GetTooltipPosition(Vector2 anchorPosition, Vector2 offset, Vector2 pivot)
		{
			float xOffset = pivot.x < 0.5f ? offset.x : -offset.x;
			float yOffset = pivot.y < 0.5f ? offset.y : -offset.y;
			return anchorPosition + new Vector2(xOffset, yOffset);
		}

		private float CalculateOverflow(Vector2 anchoredPosition, Vector2 pivot, Vector2 tooltipSize, Vector2 padding)
		{
			Rect    bounds = m_CanvasRect.rect;
			Vector2 min    = anchoredPosition - Vector2.Scale(tooltipSize, pivot);
			Vector2 max    = min + tooltipSize;
			float   left   = Mathf.Max(0.0f, bounds.xMin + padding.x - min.x);
			float   right  = Mathf.Max(0.0f, max.x - (bounds.xMax - padding.x));
			float   bottom = Mathf.Max(0.0f, bounds.yMin + padding.y - min.y);
			float   top    = Mathf.Max(0.0f, max.y - (bounds.yMax - padding.y));

			return left + right + bottom + top;
		}

		private Vector2 ClampPosition(Vector2 anchoredPosition, Vector2 pivot, Vector2 tooltipSize, Vector2 padding)
		{
			Rect  bounds = m_CanvasRect.rect;
			float minX   = bounds.xMin + padding.x + tooltipSize.x * pivot.x;
			float maxX   = bounds.xMax - padding.x - tooltipSize.x * (1.0f - pivot.x);
			float minY   = bounds.yMin + padding.y + tooltipSize.y * pivot.y;
			float maxY   = bounds.yMax - padding.y - tooltipSize.y * (1.0f - pivot.y);

			return new(
			           Mathf.Clamp(anchoredPosition.x, minX, maxX),
			           Mathf.Clamp(anchoredPosition.y, minY, maxY)
			          );
		}

		private readonly struct HoveredTrigger
		{
			public HoveredTrigger(ITooltipTrigger trigger, Component triggerComponent, Vector2 screenAnchor, bool hasWorldAnchor, Vector3 worldAnchor)
			{
				Trigger           = trigger;
				TriggerComponent  = triggerComponent;
				ScreenAnchor      = screenAnchor;
				HasWorldAnchor    = hasWorldAnchor;
				WorldAnchor       = worldAnchor;
			}

			public ITooltipTrigger Trigger { get; }
			public Component TriggerComponent { get; }
			public Vector2 ScreenAnchor { get; }
			public bool HasWorldAnchor { get; }
			public Vector3 WorldAnchor { get; }
		}
	}
}
