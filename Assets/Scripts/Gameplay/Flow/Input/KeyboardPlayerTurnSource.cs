using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Composition;
using Gameplay.Player.Domain;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Gameplay.Flow.Input
{
	public sealed class KeyboardPlayerTurnSource : IPlayerTurnSource, System.IDisposable
	{
		private const string PlayerMoveActionName = "Player/Move";

		private readonly InputAction             m_MoveAction;
		private readonly ICameraGridOrientation m_CameraGridOrientation;

		private UniTaskCompletionSource<RollDirection> m_PendingTurn;

		public KeyboardPlayerTurnSource(GameplaySceneConfiguration configuration, ICameraGridOrientation cameraGridOrientation)
		{
			if (configuration.InputActions == null) {
				throw new System.InvalidOperationException("GameplaySceneConfiguration must reference an InputActionAsset.");
			}

			m_CameraGridOrientation = cameraGridOrientation;
			m_MoveAction = configuration.InputActions.FindAction(PlayerMoveActionName, throwIfNotFound: true);
			m_MoveAction.performed += OnMovePerformed;
			m_MoveAction.Enable();
		}

		public UniTask<RollDirection> WaitForTurnAsync(CancellationToken cancellationToken)
		{
			if (m_PendingTurn != null) {
				throw new System.InvalidOperationException("Only one pending player turn is supported.");
			}

			m_PendingTurn = new UniTaskCompletionSource<RollDirection>();
			cancellationToken.Register(() => {
				UniTaskCompletionSource<RollDirection> pendingTurn = m_PendingTurn;
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
			m_MoveAction.performed -= OnMovePerformed;
			m_MoveAction.Disable();
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

				UniTaskCompletionSource<RollDirection> pendingTurn = m_PendingTurn;
				m_PendingTurn = null;
				pendingTurn.TrySetResult(direction);
			}
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
