using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using Gameplay.Nodes.Models;
using UnityEngine;


namespace Gameplay.Nodes.Runtime
{
	public interface ILevelNodeService
	{
		void                     BindLevel(NavGrid navGrid, NodeBehaviour[] nodes);
		void                     ClearLevel();
		bool                     TryGetNode(Vector2Int              cell, out NodeBehaviour node);
		NodeProjectileImpactInfo PreviewProjectileImpact(Vector2Int cell, int               incomingDamage, out NavCellOccupancy occupancy);
		void                     NotifyActorEntered(Vector2Int      cell, GameObject        actor);
		void                     NotifyActorLeft(Vector2Int         cell, GameObject        actor);
		int                      ApplyDamage(Vector2Int             cell, int               damage, GameObject source = null);
	}
}