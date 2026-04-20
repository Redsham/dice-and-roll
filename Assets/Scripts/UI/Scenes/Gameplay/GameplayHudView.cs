using Infrastructure.Services.Scenes;
using UI.Scenes.Gameplay.Elements;
using UnityEngine;
using VContainer;


namespace UI.Scenes.Gameplay
{
	public sealed class GameplayHudView : MonoBehaviour
	{
		[SerializeField] private PlayerHealthView m_PlayerHealthView;

		// === Dependencies ===

		[Inject] private SceneService m_SceneService;
		
		// === Lifecycle ===

		public void Shutdown()
		{
			if(gameObject != null) Destroy(gameObject);
		}
	}
}