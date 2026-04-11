using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Composition;
using Gameplay.Player.Domain;
using Gameplay.World.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Gameplay.Flow.Input
{
	public sealed class KeyboardPlayerTurnSource : IPlayerTurnSource, IDisposable
	{
		private const string PLAYER_MOVE_ACTION_NAME  = "Player/Move";
		private const string PLAYER_SHOOT_ACTION_NAME = "Player/Attack";

		private readonly InputAction            m_MoveAction;
		private readonly InputAction            m_ShootAction;
		private readonly ICameraGridOrientation m_CameraGridOrientation;
		private readonly ICameraScreenProjector m_CameraScreenProjector;
		private readonly INavigationService     m_NavigationService;

		private UniTaskCompletionSource<PlayerTurnCommand> m_PendingTurn;

		public KeyboardPlayerTurnSource(
			GameplaySceneConfiguration configuration,
			ICameraGridOrientation     cameraGridOrientation,
			ICameraScreenProjector     cameraScreenProjector,
			INavigationService         navigationService
		)
		{
			if (configuration.InputActions == null) {
				throw new InvalidOperationException("GameplaySceneConfiguration must reference an InputActionAsset.");
			}

			m_CameraGridOrientation =  cameraGridOrientation;
			m_CameraScreenProjector =  cameraScreenProjector;
			m_NavigationService     =  navigationService;
			m_MoveAction            =  configuration.InputActions.FindAction(PLAYER_MOVE_ACTION_NAME,  throwIfNotFound: true);
			m_ShootAction           =  configuration.InputActions.FindAction(PLAYER_SHOOT_ACTION_NAME, throwIfNotFound: true);
			m_MoveAction.performed  += OnMovePerformed;
			m_ShootAction.performed += OnShootPerformed;
			m_MoveAction.Enable();
			m_ShootAction.Enable();
		}

		public UniTask<PlayerTurnCommand> WaitForTurnAsync(CancellationToken cancellationToken)
		{
			if (m_PendingTurn != null) {
				throw new InvalidOperationException("Only one pending player turn is supported.");
			}

			m_PendingTurn = new();
			cancellationToken.Register(() => {
				UniTaskCompletionSource<PlayerTurnCommand> pendingTurn = m_PendingTurn;
				if (pendingTurn == null) {
					return;
				}

				m_PendingTurn = null;
				pendingTurn.TrySetCanceled(cancellationToken);
			});

			return m_PendingTurn.Task;
		}

		public void Dispose()
		{
			m_MoveAction.performed  -= OnMovePerformed;
			m_ShootAction.performed -= OnShootPerformed;
			m_MoveAction.Disable();
			m_ShootAction.Disable();
		}

		private void OnMovePerformed(InputAction.CallbackContext context)
		{
			if (m_PendingTurn == null) {
				return;
			}

			Vector2 input = context.ReadValue<Vector2>();
			if (TryMapLocalDirection(input, out Vector2Int localDirection)) {
				Vector2Int worldDirection = m_CameraGridOrientation.RotateLocalDirectionToWorld(localDirection);
				if (!TryMapWorldDirection(worldDirection, out RollDirection direction)) {
					return;
				}

				UniTaskCompletionSource<PlayerTurnCommand> pendingTurn = m_PendingTurn;
				m_PendingTurn = null;
				pendingTurn.TrySetResult(PlayerTurnCommand.Move(direction));
			}
		}

		private void OnShootPerformed(InputAction.CallbackContext context)
		{
			if (m_PendingTurn == null || !m_NavigationService.HasLevel) {
				return;
			}

			Vector2   screenPosition = Pointer.current?.position.ReadValue() ?? default;
			GridBasis basis          = m_NavigationService.Basis;
			if (!m_CameraScreenProjector.TryProjectScreenPointToPlane(screenPosition, basis.Origin, basis.Up, out Vector3 worldPoint)) {
				return;
			}

			UniTaskCompletionSource<PlayerTurnCommand> pendingTurn = m_PendingTurn;
			m_PendingTurn = null;
			pendingTurn.TrySetResult(PlayerTurnCommand.Shoot(worldPoint));
		}

		private static bool TryMapLocalDirection(Vector2 input, out Vector2Int direction)
		{
			const float THRESHOLD = 0.5f;

			if (input.sqrMagnitude < THRESHOLD * THRESHOLD) {
				direction = default;
				return false;
			}

			if (Mathf.Abs(input.y) >= Mathf.Abs(input.x)) {
				if (input.y > 0.0f) {
					direction = Vector2Int.up;
					return true;
				}

				direction = Vector2Int.down;
				return true;
			}

			if (input.x > 0.0f) {
				direction = Vector2Int.right;
				return true;
			}

			if (input.x < 0.0f) {
				direction = Vector2Int.left;
				return true;
			}

			direction = default;
			return false;
		}

		private static bool TryMapWorldDirection(Vector2Int direction, out RollDirection rollDirection)
		{
			if (direction == Vector2Int.up) {
				rollDirection = RollDirection.North;
				return true;
			}

			if (direction == Vector2Int.right) {
				rollDirection = RollDirection.East;
				return true;
			}

			if (direction == Vector2Int.down) {
				rollDirection = RollDirection.South;
				return true;
			}

			if (direction == Vector2Int.left) {
				rollDirection = RollDirection.West;
				return true;
			}

			rollDirection = default;
			return false;
		}
	}
}