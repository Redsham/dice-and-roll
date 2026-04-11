using UnityEngine;
using UnityEngine.UI;


namespace UI.Elements.Buttons.Components
{
	[ExecuteAlways]
	public class IconTextButtonLayoutGroup : LayoutGroup
	{
		[SerializeField] private RectTransform m_Icon;
		[SerializeField] private RectTransform m_Content;

		[Range(0f, 1f)]
		[SerializeField] private float m_IconScale = 1f;

		[SerializeField] private float m_Spacing                = 8f;
		[SerializeField] private bool  m_HideSpacingWithoutIcon = true;

		private bool HasIcon    => m_Icon    != null && m_Icon.gameObject.activeSelf;
		private bool HasContent => m_Content != null && m_Content.gameObject.activeSelf;

		private float InnerWidth  => Mathf.Max(0, rectTransform.rect.width  - padding.horizontal);
		private float InnerHeight => Mathf.Max(0, rectTransform.rect.height - padding.vertical);

		private float IconSize => HasIcon ? InnerHeight * Mathf.Clamp01(m_IconScale) : 0f;

		public float Spacing
		{
			get => m_Spacing;
			set => SetProperty(ref m_Spacing, value);
		}

		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();

			float iconSize = IconSize;
			float gap = HasIcon && HasContent || !m_HideSpacingWithoutIcon && HasContent
				            ? m_Spacing
				            : 0f;

			float contentMin  = HasContent ? LayoutUtility.GetMinWidth(m_Content) : 0f;
			float contentPref = HasContent ? LayoutUtility.GetPreferredWidth(m_Content) : 0f;

			float min  = padding.horizontal + iconSize + gap + contentMin;
			float pref = padding.horizontal + iconSize + gap + contentPref;

			SetLayoutInputForAxis(min, pref, 0f, 0);
		}

		public override void CalculateLayoutInputVertical()
		{
			float iconSize    = IconSize;
			float contentMin  = HasContent ? LayoutUtility.GetMinHeight(m_Content) : 0f;
			float contentPref = HasContent ? LayoutUtility.GetPreferredHeight(m_Content) : 0f;

			float min  = padding.vertical + Mathf.Max(iconSize, contentMin);
			float pref = padding.vertical + Mathf.Max(iconSize, contentPref);

			SetLayoutInputForAxis(min, pref, 0f, 1);
		}

		public override void SetLayoutHorizontal()
		{
			float x      = padding.left;
			float y      = padding.top;
			float height = InnerHeight;

			if (HasIcon) {
				float iconSize = IconSize;
				float iconY    = y + (height - iconSize) * 0.5f;

				SetChildAlongAxis(m_Icon, 0, x,     iconSize);
				SetChildAlongAxis(m_Icon, 1, iconY, iconSize);

				x += iconSize;

				if (HasContent || !m_HideSpacingWithoutIcon)
					x += m_Spacing;
			}

			if (HasContent) {
				float width = Mathf.Max(0, rectTransform.rect.width - padding.right - x);

				SetChildAlongAxis(m_Content, 0, x, width);
				SetChildAlongAxis(m_Content, 1, y, height);
			}
		}

		public override void SetLayoutVertical()
		{

		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SetDirty();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			SetDirty();
		}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			m_IconScale = Mathf.Clamp01(m_IconScale);
			SetDirty();
		}
		#endif
	}
}