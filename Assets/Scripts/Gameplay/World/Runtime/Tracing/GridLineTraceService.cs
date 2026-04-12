using Gameplay.Actors.Runtime;
using Gameplay.Navigation;
using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.World.Runtime.Tracing
{
	public sealed class GridLineTraceService : IGridLineTraceService
	{
		private readonly INavigationService     m_NavigationService;
		private readonly ICombatResolverService m_CombatResolverService;

		public GridLineTraceService(INavigationService navigationService, ICombatResolverService combatResolverService)
		{
			m_NavigationService     = navigationService;
			m_CombatResolverService = combatResolverService;
		}

		public GridTraceResult Trace(Vector2Int origin, RollDirection direction, int maxDistance)
		{
			Vector2Int currentCell   = origin;
			Vector2Int stepDirection = ToStepDirection(direction);

			for (int distance = 1; distance <= maxDistance; distance++) {
				currentCell += stepDirection;
				if (!m_NavigationService.TryGetOccupancy(currentCell, out NavCellOccupancy occupancy)) {
					return new(distance, false, true, currentCell);
				}

				bool hitActor = m_CombatResolverService.IsCellOccupiedByActor(currentCell);
				bool hitNode  = occupancy.Type != NavCellOccupancyType.Empty;
				if (hitActor || hitNode) {
					return new(distance, true, false, currentCell);
				}
			}

			return new(maxDistance, false, false, currentCell);
		}

		private static Vector2Int ToStepDirection(RollDirection direction)
		{
			return direction switch {
				RollDirection.North => Vector2Int.up,
				RollDirection.East  => Vector2Int.right,
				RollDirection.South => Vector2Int.down,
				RollDirection.West  => Vector2Int.left,
				_                   => default
			};
		}
	}
}
