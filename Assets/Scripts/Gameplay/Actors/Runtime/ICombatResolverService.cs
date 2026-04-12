using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public interface ICombatResolverService
	{
		// === Lifecycle ===

		void Clear();
		void RegisterActor(IGridActor   actor);
		void UnregisterActor(IGridActor actor);
		void MoveActor(IGridActor       actor, Vector2Int from, Vector2Int to);

		// === Queries ===

		int  ApplyDamage(INavCellEntity entity, int damage, GameObject source = null);
		int  ApplyDamage(Vector2Int cell, int damage, GameObject source = null);
	}
}
