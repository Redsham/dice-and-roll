using Cysharp.Threading.Tasks;
using Infrastructure.Services.Scenes;
using UI.Effects;
using UnityEditor;
using UnityEngine;
using VContainer;


namespace UI.Scenes.Menu
{
	public class MainMenuView : MonoBehaviour
	{
		[Inject] private readonly SceneService m_SceneService;
		[Inject] private readonly UIFade       m_Fade;

		public void Play()
		{
			Debug.Log($"[{nameof(MainMenuView)}] Starting game...");
			m_Fade.ActionAsync(() => m_SceneService.LoadSceneAsync("Gameplay")).Forget();
		}



		public void Quit()
		{
			Debug.Log($"[{nameof(MainMenuView)}] Quitting...");

			m_Fade.Action(() => {
				#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
				#else
				Application.Quit();
				#endif
			}).Forget();
		}
	}
}