using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using Gameplay.Navigation.Pathfinding;
using UnityEngine;


namespace Gameplay.World.Runtime
{
	public interface INavigationService
	{
		GridBasis Basis    { get; }
		bool      HasLevel { get; }

		void BindLevel(LevelBehaviour level);
		void ClearLevel();
		bool CanOccupy(Vector2Int       coordinates);
		bool TryGetOccupancy(Vector2Int coordinates, out NavCellOccupancy occupancy);
		bool TryFindPath(Vector2Int     start,       Vector2Int           goal, int[] pathBuffer, out NavPathResult result);
		bool TryFindPath(Vector2Int     start,       Vector2Int           goal, Vector2Int[] pathBuffer, out NavPathResult result);
		bool TryFindPath<TWeightProvider>(Vector2Int start, Vector2Int goal, ref TWeightProvider weightProvider, Vector2Int[] pathBuffer, out NavPathResult result)
			where TWeightProvider : struct, Gameplay.Navigation.Pathfinding.Providers.INavTraversalCostProvider;
	}
}
