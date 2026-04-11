using Gameplay.Navigation;
using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.World.Runtime.Tracing
{
	public interface IGridLineTraceService
	{
		NavLineTraceResult Trace(Vector2Int origin, RollDirection direction, int maxDistance, int initialPower, NavLineTraceStep[] stepBuffer = null);
	}
}
