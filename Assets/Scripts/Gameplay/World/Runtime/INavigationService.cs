using Gameplay.Levels.Authoring;
using Gameplay.Navigation.Pathfinding;
using UnityEngine;


namespace Gameplay.World.Runtime
{
	public interface INavigationService
	{
		GridBasis Basis { get; }
		bool HasLevel { get; }

		void BindLevel(LevelBehaviour level);
		void ClearLevel();
		bool CanOccupy(Vector2Int coordinates);
		bool TryFindPath(Vector2Int start, Vector2Int goal, int[] pathBuffer, out NavPathResult result);
	}
}
