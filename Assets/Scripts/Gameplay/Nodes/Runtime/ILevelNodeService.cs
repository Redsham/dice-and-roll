using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using UnityEngine;


namespace Gameplay.Nodes.Runtime
{
	public interface ILevelNodeService
	{
		void                     BindLevel(NavGrid navGrid, TileBehaviour[] tiles);
		void                     ClearLevel();
		bool                     TryGetTile(Vector2Int              cell, out TileBehaviour tile);
		void                     NotifyActorEntered(Vector2Int      cell, GameObject        actor);
		void                     NotifyActorLeft(Vector2Int         cell, GameObject        actor);
	}
}
