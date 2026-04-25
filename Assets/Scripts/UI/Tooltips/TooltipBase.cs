using System;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Tooltips
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public abstract class TooltipBase : MonoBehaviour
	{
		[SerializeField] private RectTransform m_PanelRoot;
		[SerializeField] private CanvasGroup   m_CanvasGroup;
		[SerializeField] private RectTransform m_AnchorDot;
		[SerializeField] private UILine        m_Line;

		[Header("Animation")]
		[SerializeField, Min(0.01f)] private float   m_ShowDuration = 0.18f;
		[SerializeField, Min(0.01f)] private float   m_HideDuration = 0.12f;
		[SerializeField]             private Ease    m_ShowEase     = Ease.OutCubic;
		[SerializeField]             private Ease    m_HideEase     = Ease.InCubic;
		[SerializeField]             private Vector3 m_HiddenScale  = new(0.94f, 0.94f, 1.0f);

		private MotionHandle m_AlphaHandle;
		private MotionHandle m_ScaleHandle;
		private Action       m_HideCompleted;
		private Vector2      m_AnchorCanvasPosition;
		private bool         m_CalloutVisible;

		protected RectTransform RootRect  { get; private set; }
		public    RectTransform PanelRect => m_PanelRoot != null ? m_PanelRoot : RootRect;

		protected virtual void Awake()
		{
			RootRect = (RectTransform)transform;
			if (m_CanvasGroup == null) {
				m_CanvasGroup = GetComponent<CanvasGroup>();
			}

			if (m_CanvasGroup == null) {
				m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
			}

			m_CanvasGroup.blocksRaycasts = false;
			m_CanvasGroup.interactable   = false;
		}

		protected virtual void OnDestroy()
		{
			CancelInvoke(nameof(InvokeHideCompleted));
			m_AlphaHandle.TryCancel();
			m_ScaleHandle.TryCancel();
		}

		public virtual void RefreshLayout()
		{
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(PanelRect);
		}

		public void SetPlacement(Vector2 anchoredPosition, Vector2 pivot)
		{
			RectTransform panelRect = PanelRect;
			panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
			panelRect.pivot = pivot;
			panelRect.anchoredPosition = anchoredPosition;

			RefreshCallout();
		}

		public void SetCallout(Vector2 anchorCanvasPosition, bool isVisible)
		{
			m_AnchorCanvasPosition = anchorCanvasPosition;
			m_CalloutVisible       = isVisible;

			RefreshCallout();
		}

		public void PlayShow()
		{
			gameObject.SetActive(true);
			m_HideCompleted = null;
			CancelInvoke(nameof(InvokeHideCompleted));

			m_AlphaHandle.TryCancel();
			m_ScaleHandle.TryCancel();

			m_CanvasGroup.alpha       = 0.0f;
			PanelRect.localScale      = m_HiddenScale;
			PanelRect.localEulerAngles = Vector3.zero;

			m_AlphaHandle = LMotion.Create(0.0f, 1.0f, m_ShowDuration)
			                       .WithEase(m_ShowEase)
			                       .Bind(alpha => m_CanvasGroup.alpha = alpha)
			                       .AddTo(this);

			m_ScaleHandle = LMotion.Create(m_HiddenScale, Vector3.one, m_ShowDuration)
			                       .WithEase(m_ShowEase)
			                       .Bind(scale => PanelRect.localScale = scale)
			                       .AddTo(this);
		}

		public void PlayHide(Action onCompleted)
		{
			m_HideCompleted = onCompleted;
			CancelInvoke(nameof(InvokeHideCompleted));

			m_AlphaHandle.TryCancel();
			m_ScaleHandle.TryCancel();

			m_AlphaHandle = LMotion.Create(m_CanvasGroup.alpha, 0.0f, m_HideDuration)
			                       .WithEase(m_HideEase)
			                       .Bind(alpha => m_CanvasGroup.alpha = alpha)
			                       .AddTo(this);

			m_ScaleHandle = LMotion.Create(PanelRect.localScale, m_HiddenScale, m_HideDuration)
			                       .WithEase(m_HideEase)
			                       .Bind(scale => PanelRect.localScale = scale)
			                       .AddTo(this);

			Invoke(nameof(InvokeHideCompleted), m_HideDuration);
		}

		private void RefreshCallout()
		{
			if (m_AnchorDot != null) {
				m_AnchorDot.gameObject.SetActive(m_CalloutVisible);
				if (m_CalloutVisible) {
					m_AnchorDot.anchorMin = m_AnchorDot.anchorMax = new Vector2(0.5f, 0.5f);
					m_AnchorDot.anchoredPosition = m_AnchorCanvasPosition;
				}
			}

			if (m_Line != null) {
				m_Line.gameObject.SetActive(m_CalloutVisible);
				if (m_CalloutVisible) {
					m_Line.SetPoints(m_AnchorCanvasPosition, GetClosestPointOnPanel(m_AnchorCanvasPosition));
				}
			}
		}

		private Vector2 GetClosestPointOnPanel(Vector2 anchorCanvasPosition)
		{
			RectTransform panelRect  = PanelRect;
			Vector2       panelSize  = panelRect.rect.size;
			Vector2       panelMin   = panelRect.anchoredPosition - Vector2.Scale(panelSize, panelRect.pivot);
			Vector2       panelMax   = panelMin + panelSize;
			float         clampedX   = Mathf.Clamp(anchorCanvasPosition.x, panelMin.x, panelMax.x);
			float         clampedY   = Mathf.Clamp(anchorCanvasPosition.y, panelMin.y, panelMax.y);

			return new(clampedX, clampedY);
		}

		private void InvokeHideCompleted()
		{
			Action callback = m_HideCompleted;
			m_HideCompleted = null;
			callback?.Invoke();
		}
	}
}
