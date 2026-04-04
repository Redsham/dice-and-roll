using UnityEngine;
using UnityEngine.UI;


namespace UI.Elements.Buttons.Components
{
    [ExecuteAlways]
    public class IconTextButtonLayoutGroup : LayoutGroup
    {
        [SerializeField] private RectTransform m_Icon;
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private float         m_Spacing = 8f;
        [SerializeField] private bool          m_HideSpacingWithoutIcon = true;

        private bool HasIcon    => m_Icon != null && m_Icon.gameObject.activeSelf;
        private bool HasContent => m_Content != null && m_Content.gameObject.activeSelf;

        private float InnerWidth  => rectTransform.rect.width  - padding.horizontal;
        private float InnerHeight => rectTransform.rect.height - padding.vertical;

        public float Spacing
        {
            get => m_Spacing;
            set => SetProperty(ref m_Spacing, value);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            float iconSize = HasIcon ? Mathf.Max(0, InnerHeight) : 0f;
            float gap      = (HasIcon && HasContent) || (!m_HideSpacingWithoutIcon && HasContent) ? m_Spacing : 0f;

            float contentMin  = HasContent ? LayoutUtility.GetMinWidth(m_Content) : 0f;
            float contentPref = HasContent ? LayoutUtility.GetPreferredWidth(m_Content) : 0f;

            float min  = padding.horizontal + iconSize + gap + contentMin;
            float pref = padding.horizontal + iconSize + gap + contentPref;

            SetLayoutInputForAxis(min, pref, 0f, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            float iconSize    = HasIcon ? Mathf.Max(0, InnerHeight) : 0f;
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
            float height = Mathf.Max(0, InnerHeight);

            if (HasIcon)
            {
                float iconSize = height;

                SetChildAlongAxis(m_Icon, 0, x, iconSize);
                SetChildAlongAxis(m_Icon, 1, y, height);

                x += iconSize;

                if (HasContent || !m_HideSpacingWithoutIcon)
                    x += m_Spacing;
            }

            if (HasContent)
            {
                float width = Mathf.Max(0, rectTransform.rect.width - padding.right - x);

                SetChildAlongAxis(m_Content, 0, x, width);
                SetChildAlongAxis(m_Content, 1, y, height);
            }
        }

        public override void SetLayoutVertical()
        {
            
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif
    }
}