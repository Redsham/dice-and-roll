using Cysharp.Threading.Tasks;
using Infrastructure.Services.Scenes;
using UI.Effects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;


public class ShowcaseFunctions : ITickable
{
	[Inject] private readonly UIFade       m_Fade;
	[Inject] private readonly SceneService m_SceneService;

	public void Tick()
	{
		if (Keyboard.current.f1Key.wasPressedThisFrame) {
			m_Fade.ActionAsync(() => m_SceneService.LoadSceneAsync(SceneManager.GetActiveScene().name)).Forget();
			Debug.Log($"[{nameof(ShowcaseFunctions)}] Reloading scene: {SceneManager.GetActiveScene().name}");
		}


		if (Keyboard.current.escapeKey.wasPressedThisFrame) {
			m_Fade.ActionAsync(() => m_SceneService.LoadSceneAsync("Menu")).Forget();
			Debug.Log($"[{nameof(ShowcaseFunctions)}] Returning to menu.");
		}
	}
}