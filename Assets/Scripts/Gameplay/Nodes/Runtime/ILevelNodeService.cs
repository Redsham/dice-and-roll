using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using Gameplay.Nodes.Models;
using UnityEngine;


namespace Gameplay.Nodes.Runtime
{
	public interface ILevelNodeService
	{
		void                     BindLevel(NavGrid navGrid, TileBehaviour[] tiles);
		void                     ClearLevel();
		bool                     TryGetTile(Vector2Int              cell, out TileBehaviour tile);
		NodeProjectileImpactInfo PreviewProjectileImpact(Vector2Int cell, int               incomingDamage, out NavCellOccupancy occupancy);
		void                     NotifyActorEntered(Vector2Int      cell, GameObject        actor);
		void                     NotifyActorLeft(Vector2Int         cell, GameObject        actor);
		int                      ApplyDamage(Vector2Int             cell, int               damage, GameObject source = null);
	}
}
