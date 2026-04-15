using Infrastructure.Services.Scenes;
using UnityEngine;
using VContainer;


namespace UI.Gameplay
{
	public sealed class GameplayHudView : MonoBehaviour
	{
		[SerializeField] private PlayerHealthView m_PlayerHealthView;

		// === Dependencies ===

		[Inject] private SceneService m_SceneService;
		
		// === Lifecycle ===
		
		public void Shutdown() => Destroy(gameObject);
	}
}