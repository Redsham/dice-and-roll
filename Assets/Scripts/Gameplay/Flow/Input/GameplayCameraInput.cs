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
		
		private const float  HORIZONTAL_SCROLL_DEAD_ZONE = 0.5f;
		private const float  HORIZONTAL_SCROLL_COMMIT_THRESHOLD = 38.0f;
		private const float  HORIZONTAL_SCROLL_PREVIEW_MAX_ANGLE = 34.0f;
		private const float  HORIZONTAL_SCROLL_RETURN_SPEED = 420.0f;
		private const float  HORIZONTAL_SCROLL_ACTIVE_TIMEOUT = 0.08f;
		private const float  MOUSE_WHEEL_SCROLL_THRESHOLD = 10.0f;
		private const float  TOUCHPAD_ZOOM_SCROLL_SCALE = 0.25f;
		private const float  RIGHT_MOUSE_DRAG_DEAD_ZONE = 0.5f;
		private const float  RIGHT_MOUSE_DRAG_COMMIT_THRESHOLD = 120.0f;

		private readonly IGameCameraController m_GameCameraController;
		private readonly InputAction           m_PreviousAction;
		private readonly InputAction           m_NextAction;
		private readonly InputAction           m_ScrollAction;
		private float                          m_HorizontalScrollGesture;
		private float                          m_LastHorizontalScrollTime = float.NegativeInfinity;
		private bool                           m_HorizontalScrollCommitted;
		private float                          m_RightMouseDragGesture;
		private bool                           m_RightMouseDragCommitted;

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
			ResetHorizontalScrollGesture();
			ResetRightMouseDragGesture();
			m_GameCameraController.RotateOrbitRight();
		}

		private void OnNextPerformed(InputAction.CallbackContext context)
		{
			ResetHorizontalScrollGesture();
			ResetRightMouseDragGesture();
			m_GameCameraController.RotateOrbitLeft();
		}

		private void OnScrollPerformed(InputAction.CallbackContext context)
		{
			Vector2 scroll = context.ReadValue<Vector2>();
			if (TryHandleHorizontalScroll(scroll)) {
				return;
			}

			HandleVerticalScrollZoom(scroll.y);
		}

		private bool TryHandleHorizontalScroll(Vector2 scroll)
		{
			if (!IsHorizontalScrollIntent(scroll)) {
				return false;
			}

			m_LastHorizontalScrollTime = Time.unscaledTime;
			if (m_HorizontalScrollCommitted) {
				return true;
			}

			m_HorizontalScrollGesture = Mathf.Clamp(
			                                       m_HorizontalScrollGesture + scroll.x,
			                                       -HORIZONTAL_SCROLL_COMMIT_THRESHOLD,
			                                       HORIZONTAL_SCROLL_COMMIT_THRESHOLD
			                                      );

			if (Mathf.Abs(m_HorizontalScrollGesture) >= HORIZONTAL_SCROLL_COMMIT_THRESHOLD) {
				if (m_HorizontalScrollGesture > 0.0f) {
					m_GameCameraController.RotateOrbitLeft();
				}
				else {
					m_GameCameraController.RotateOrbitRight();
				}

				ResetHorizontalScrollGesture();
				m_HorizontalScrollCommitted = true;
				return true;
			}

			ApplyHorizontalScrollPreview();
			return true;
		}

		private bool IsHorizontalScrollIntent(Vector2 scroll)
		{
			float horizontal = Mathf.Abs(scroll.x);
			return horizontal >= HORIZONTAL_SCROLL_DEAD_ZONE && horizontal > Mathf.Abs(scroll.y);
		}

		private void ApplyHorizontalScrollPreview()
		{
			float progress = Mathf.Clamp(
			                             m_HorizontalScrollGesture / HORIZONTAL_SCROLL_COMMIT_THRESHOLD,
			                             -1.0f,
			                             1.0f
			                            );
			m_GameCameraController.SetOrbitRotationPreview(-progress * HORIZONTAL_SCROLL_PREVIEW_MAX_ANGLE);
		}

		private void ResetHorizontalScrollGesture()
		{
			m_HorizontalScrollGesture = 0.0f;
			m_HorizontalScrollCommitted = false;
			m_GameCameraController.SetOrbitRotationPreview(0.0f);
		}

		private void HandleVerticalScrollZoom(float verticalScroll)
		{
			if (Mathf.Approximately(verticalScroll, 0.0f)) {
				return;
			}

			float zoomInput = Mathf.Abs(verticalScroll) >= MOUSE_WHEEL_SCROLL_THRESHOLD
				                  ? Mathf.Sign(verticalScroll)
				                  : verticalScroll * TOUCHPAD_ZOOM_SCROLL_SCALE;
			m_GameCameraController.AdjustOrbitZoom(zoomInput);
		}

		public void Tick()
		{
			TickRightMouseDrag();
			TickHorizontalScrollGestureReturn();

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

		private void TickHorizontalScrollGestureReturn()
		{
			bool isGestureInactive = Time.unscaledTime - m_LastHorizontalScrollTime >= HORIZONTAL_SCROLL_ACTIVE_TIMEOUT;
			if (isGestureInactive) {
				m_HorizontalScrollCommitted = false;
			}

			if (Mathf.Approximately(m_HorizontalScrollGesture, 0.0f)) {
				return;
			}

			if (!isGestureInactive) {
				return;
			}

			m_HorizontalScrollGesture = Mathf.MoveTowards(
			                                             m_HorizontalScrollGesture,
			                                             0.0f,
			                                             HORIZONTAL_SCROLL_RETURN_SPEED * Time.unscaledDeltaTime
			                                            );
			ApplyHorizontalScrollPreview();
		}

		private void TickRightMouseDrag()
		{
			Mouse mouse = Mouse.current;
			if (mouse == null) {
				return;
			}

			if (!mouse.rightButton.isPressed) {
				if (!Mathf.Approximately(m_RightMouseDragGesture, 0.0f)) {
					m_RightMouseDragGesture = Mathf.MoveTowards(
					                                            m_RightMouseDragGesture,
					                                            0.0f,
					                                            HORIZONTAL_SCROLL_RETURN_SPEED * Time.unscaledDeltaTime
					                                           );
					ApplyRightMouseDragPreview();
				}

				m_RightMouseDragCommitted = false;
				return;
			}

			if (m_RightMouseDragCommitted) {
				return;
			}

			Vector2 delta = mouse.delta.ReadValue();
			if (Mathf.Abs(delta.x) < RIGHT_MOUSE_DRAG_DEAD_ZONE || Mathf.Abs(delta.x) <= Mathf.Abs(delta.y)) {
				return;
			}

			ResetHorizontalScrollGesture();
			m_RightMouseDragGesture = Mathf.Clamp(
			                                      m_RightMouseDragGesture + delta.x,
			                                      -RIGHT_MOUSE_DRAG_COMMIT_THRESHOLD,
			                                      RIGHT_MOUSE_DRAG_COMMIT_THRESHOLD
			                                     );

			if (Mathf.Abs(m_RightMouseDragGesture) >= RIGHT_MOUSE_DRAG_COMMIT_THRESHOLD) {
				if (m_RightMouseDragGesture > 0.0f) {
					m_GameCameraController.RotateOrbitRight();
				}
				else {
					m_GameCameraController.RotateOrbitLeft();
				}

				ResetRightMouseDragGesture();
				m_RightMouseDragCommitted = true;
				return;
			}

			ApplyRightMouseDragPreview();
		}

		private void ApplyRightMouseDragPreview()
		{
			float progress = Mathf.Clamp(
			                             m_RightMouseDragGesture / RIGHT_MOUSE_DRAG_COMMIT_THRESHOLD,
			                             -1.0f,
			                             1.0f
			                            );
			m_GameCameraController.SetOrbitRotationPreview(progress * HORIZONTAL_SCROLL_PREVIEW_MAX_ANGLE);
		}

		private void ResetRightMouseDragGesture()
		{
			m_RightMouseDragGesture = 0.0f;
			m_RightMouseDragCommitted = false;
			m_GameCameraController.SetOrbitRotationPreview(0.0f);
		}
	}
}
