using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Object = UnityEngine.Object;


namespace UI.Effects
{
	public class UIFade
	{
		private readonly GameObject m_Root  = null;
		private readonly Image      m_Image = null;
		private readonly Color      m_Color = ColorUtilities.FromHex("#260101");

		public UIFade()
		{
			GameObject canvasObj = m_Root = new("UIFadeCanvas");
			Canvas     canvas    = canvasObj.AddComponent<Canvas>();
			canvas.AddComponent<CanvasRenderer>();
			canvasObj.AddComponent<CanvasScaler>();

			canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 1000;
			Object.DontDestroyOnLoad(canvasObj);

			GameObject imageObj = new("UIFadeImage");
			imageObj.transform.SetParent(canvas.transform);

			Image image = m_Image = imageObj.AddComponent<Image>();

			RectTransform imageRect = image.rectTransform;
			imageRect.anchorMin        = Vector2.zero;
			imageRect.anchorMax        = Vector2.one;
			imageRect.sizeDelta        = Vector2.zero;
			imageRect.anchoredPosition = Vector2.zero;

			m_Root.SetActive(false);

			Debug.Log("UIFade initialized.");
		}

		public async UniTask Show(float duration = 0.5f)
		{
			m_Root.SetActive(true);

			Color clearColor = m_Color;
			clearColor.a  = 0.0f;
			m_Image.color = clearColor;

			await LMotion.Create(clearColor, m_Color, duration)
			             .WithEase(Ease.InBack)
			             .Bind(color => m_Image.color = color);
		}
		public async UniTask Hide(float duration = 0.5f)
		{
			Color clearColor = m_Image.color;
			clearColor.a  = 0.0f;

			await LMotion.Create(m_Image.color, clearColor, duration)
			             .WithEase(Ease.InBack)
			             .Bind(color => m_Image.color = color);

			m_Root.SetActive(false);
		}

		public async UniTask Action(Action action, float duration = 1.0f)
		{
			await Show(duration / 2.0f);
			action();
			await Hide(duration / 2.0f);
		}
		public async UniTask ActionAsync(Func<UniTask> action, float duration = 1.0f)
		{
			await Show(duration / 2.0f);
			await action();
			await Hide(duration / 2.0f);
		}
	}
}