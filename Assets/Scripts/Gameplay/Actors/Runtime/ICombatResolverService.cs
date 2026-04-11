using Gameplay.Navigation;
using Gameplay.Nodes.Models;
using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public interface ICombatResolverService
	{
		// === Lifecycle ===

		void Clear();
		void RegisterActor(IGridActor actor);
		void UnregisterActor(IGridActor actor);
		void MoveActor(IGridActor actor, Vector2Int from, Vector2Int to);

		// === Queries ===

		bool IsCellOccupiedByActor(Vector2Int cell);
		NodeProjectileImpactInfo PreviewProjectileImpact(Vector2Int cell, int incomingDamage, out NavCellOccupancy occupancy);
		int ApplyDamage(Vector2Int cell, int damage, GameObject source = null);
	}
}
