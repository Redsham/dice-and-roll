using UnityEngine;


namespace Gameplay.Navigation.Tracing
{
	public static class NavGridLineTrace
	{
		public static GridTraceResult Trace(NavGrid navGrid, Vector2Int origin, Vector2Int direction, int maxDistance)
		{
			if (navGrid == null) {
				return new(0, false, true, origin);
			}

			if (direction == Vector2Int.zero) {
				return new(0, false, false, origin);
			}

			Vector2Int currentCell = origin;

			for (int distance = 1; distance <= maxDistance; distance++) {
				currentCell += direction;
				if (!navGrid.TryGetEntity(currentCell, out INavCellEntity entity)) {
					return new(distance, false, true, currentCell);
				}

				if (entity != null && entity.IsAlive) {
					return new(distance, true, false, currentCell, entity);
				}
			}

			return new(maxDistance, false, false, currentCell);
		}
	}
}
