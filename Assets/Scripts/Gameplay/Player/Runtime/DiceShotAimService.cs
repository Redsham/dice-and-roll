using Gameplay.Camera.Abstractions;
using Gameplay.Navigation.Tracing;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.World.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;


namespace Gameplay.Player.Runtime
{
	public sealed class DiceShotAimService
	{
		[Inject] private readonly ICameraScreenProjector m_CameraScreenProjector;
		[Inject] private readonly INavigationService     m_NavigationService;

		public bool TryGetPointerAimPoint(out Vector3 worldPoint)
		{
			if (Pointer.current == null || !m_NavigationService.HasLevel) {
				worldPoint = default;
				return false;
			}

			GridBasis basis          = m_NavigationService.Basis;
			Vector2   screenPosition = Pointer.current.position.ReadValue();
			return m_CameraScreenProjector.TryProjectScreenPointToPlane(screenPosition, basis.Origin, basis.Up, out worldPoint);
		}

		public bool TryResolvePointerShot(DiceState state, out DiceShotDefinition shot)
		{
			if (!TryGetPointerAimPoint(out Vector3 worldPoint)) {
				shot = default;
				return false;
			}

			return state.TryResolveShot(m_NavigationService.Basis, worldPoint, out shot);
		}

		public bool TryResolvePointerShotTrace(DiceState state, int maxDistance, out DiceShotDefinition shot, out GridTraceResult traceResult)
		{
			if (!TryResolvePointerShot(state, out shot)) {
				traceResult = default;
				return false;
			}

			traceResult = NavGridLineTrace.Trace(
			                                     m_NavigationService.Grid,
			                                     state.Position,
			                                     shot.Direction.ToVector2Int(),
			                                     maxDistance
			                                    );
			return true;
		}
	}
}