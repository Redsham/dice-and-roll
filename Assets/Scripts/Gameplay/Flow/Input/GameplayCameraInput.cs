using System;
using Gameplay.Camera.Abstractions;
using Gameplay.Composition;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;


namespace Gameplay.Flow.Input
{
	public sealed class GameplayCameraInput : IInitializable, ITickable, IDisposable
	{
		private const string PREVIOUS_ACTION_NAME = "Player/Previous";
		private const string NEXT_ACTION_NAME     = "Player/Next";
		private const string SCROLL_ACTION_NAME   = "UI/ScrollWheel";

		private readonly IGameCameraController m_GameCameraController;
		private readonly InputAction           m_PreviousAction;
		private readonly InputAction           m_NextAction;
		private readonly InputAction           m_ScrollAction;

		public GameplayCameraInput(GameplaySceneConfiguration configuration, IGameCameraController gameCameraController)
		{
			if (configuration.InputActions == null) {
				throw new InvalidOperationException("GameplaySceneConfiguration must reference an InputActionAsset.");
			}

			m_GameCameraController = gameCameraController;
			m_PreviousAction       = configuration.InputActions.FindAction(PREVIOUS_ACTION_NAME, throwIfNotFound: true);
			m_NextAction           = configuration.InputActions.FindAction(NEXT_ACTION_NAME,     throwIfNotFound: true);
			m_ScrollAction         = configuration.InputActions.FindAction(SCROLL_ACTION_NAME,   throwIfNotFound: true);
		}

		public void Initialize()
		{
			m_PreviousAction.performed += OnPreviousPerformed;
			m_NextAction.performed     += OnNextPerformed;
			m_ScrollAction.performed   += OnScrollPerformed;
			m_PreviousAction.Enable();
			m_NextAction.Enable();
			m_ScrollAction.Enable();
		}

		public void Dispose()
		{
			m_PreviousAction.performed -= OnPreviousPerformed;
			m_NextAction.performed     -= OnNextPerformed;
			m_ScrollAction.performed   -= OnScrollPerformed;
			m_PreviousAction.Disable();
			m_NextAction.Disable();
			m_ScrollAction.Disable();
		}

		private void OnPreviousPerformed(InputAction.CallbackContext context)
		{
			m_GameCameraController.RotateOrbitRight();
		}

		private void OnNextPerformed(InputAction.CallbackContext context)
		{
			m_GameCameraController.RotateOrbitLeft();
		}

		private void OnScrollPerformed(InputAction.CallbackContext context)
		{
			Vector2 scroll = context.ReadValue<Vector2>();
			if (!Mathf.Approximately(scroll.y, 0.0f)) {
				m_GameCameraController.AdjustOrbitZoom(scroll.y > 0.0f ? 1.0f : -1.0f);
			}
		}

		public void Tick()
		{
			Gamepad gamepad = Gamepad.current;
			if (gamepad == null) {
				return;
			}

			Vector2 look     = gamepad.rightStick.ReadValue();
			float   vertical = look.y;
			if (Mathf.Abs(vertical) < 0.35f) {
				return;
			}

			m_GameCameraController.AdjustOrbitZoom(vertical * Time.deltaTime * 6.0f);
		}
	}
}