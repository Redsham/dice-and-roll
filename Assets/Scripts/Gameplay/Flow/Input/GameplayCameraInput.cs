using Gameplay.Camera.Abstractions;
using Gameplay.Composition;
using UnityEngine.InputSystem;
using VContainer.Unity;


namespace Gameplay.Flow.Input
{
	public sealed class GameplayCameraInput : IInitializable, System.IDisposable
	{
		private const string PREVIOUS_ACTION_NAME = "Player/Previous";
		private const string NEXT_ACTION_NAME     = "Player/Next";

		private readonly IGameCameraController m_GameCameraController;
		private readonly InputAction           m_PreviousAction;
		private readonly InputAction           m_NextAction;

		public GameplayCameraInput(GameplaySceneConfiguration configuration, IGameCameraController gameCameraController)
		{
			if (configuration.InputActions == null) {
				throw new System.InvalidOperationException("GameplaySceneConfiguration must reference an InputActionAsset.");
			}

			m_GameCameraController = gameCameraController;
			m_PreviousAction = configuration.InputActions.FindAction(PREVIOUS_ACTION_NAME, throwIfNotFound: true);
			m_NextAction     = configuration.InputActions.FindAction(NEXT_ACTION_NAME, throwIfNotFound: true);
		}

		public void Initialize()
		{
			m_PreviousAction.performed += OnPreviousPerformed;
			m_NextAction.performed     += OnNextPerformed;
			m_PreviousAction.Enable();
			m_NextAction.Enable();
		}

		public void Dispose()
		{
			m_PreviousAction.performed -= OnPreviousPerformed;
			m_NextAction.performed     -= OnNextPerformed;
			m_PreviousAction.Disable();
			m_NextAction.Disable();
		}

		private void OnPreviousPerformed(InputAction.CallbackContext context)
		{
			m_GameCameraController.RotateOrbitRight();
		}

		private void OnNextPerformed(InputAction.CallbackContext context)
		{
			m_GameCameraController.RotateOrbitLeft();
		}
	}
}
