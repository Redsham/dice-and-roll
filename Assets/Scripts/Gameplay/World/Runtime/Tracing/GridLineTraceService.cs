using Gameplay.Navigation;
using Gameplay.Actors.Runtime;
using Gameplay.Nodes.Models;
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
			m_NavigationService = navigationService;
			m_CombatResolverService = combatResolverService;
		}

		public NavLineTraceResult Trace(Vector2Int origin, RollDirection direction, int maxDistance, int initialPower, NavLineTraceStep[] stepBuffer = null)
		{
			int        remainingPower = Mathf.Max(0, initialPower);
			int        stepCount      = 0;
			bool       wasStopped     = false;
			bool       exitedBounds   = false;
			Vector2Int currentCell    = origin;
			Vector2Int stepDirection  = ToStepDirection(direction);

			for (int distance = 1; distance <= maxDistance && remainingPower > 0; distance++) {
				currentCell += stepDirection;
				if (!m_NavigationService.TryGetOccupancy(currentCell, out _)) {
					exitedBounds = true;
					break;
				}
				
				NodeProjectileImpactInfo impactInfo = m_CombatResolverService.PreviewProjectileImpact(currentCell, remainingPower, out NavCellOccupancy occupancy);

				int powerBefore   = remainingPower;
				int powerConsumed = impactInfo.ConsumedDamage;
				remainingPower = Mathf.Max(0, powerBefore - powerConsumed);
				bool stopsTrace = impactInfo.StopsProjectile || remainingPower <= 0;

				if (stepBuffer != null && stepCount < stepBuffer.Length) {
					stepBuffer[stepCount] = new NavLineTraceStep(
						currentCell,
						occupancy,
						distance,
						powerBefore,
						powerConsumed,
						remainingPower,
						stopsTrace
					);
				}

				stepCount++;

				if (stopsTrace) {
					wasStopped = true;
					break;
				}
			}

			return new NavLineTraceResult(stepCount, remainingPower, wasStopped, exitedBounds, currentCell);
		}

		private static Vector2Int ToStepDirection(RollDirection direction)
		{
			return direction switch
			{
				RollDirection.North => Vector2Int.up,
				RollDirection.East => Vector2Int.right,
				RollDirection.South => Vector2Int.down,
				RollDirection.West => Vector2Int.left,
				_ => default
			};
		}
	}
}
