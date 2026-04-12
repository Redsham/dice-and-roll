using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public interface IGridActor
	{
		// === Identity ===

		GameObject Owner { get; }

		// === Spatial ===

		Vector2Int       Cell      { get; }
		NavCellOccupancy Occupancy { get; }

		// === Combat ===

		bool IsAlive { get; }
		int  ApplyDamage(int damage, GameObject source = null);
	}
}
