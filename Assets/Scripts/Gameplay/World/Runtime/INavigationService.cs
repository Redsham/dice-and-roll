using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using Gameplay.Navigation.Pathfinding;
using UnityEngine;


namespace Gameplay.World.Runtime
{
	public interface INavigationService
	{
		NavGrid   Grid     { get; }
		GridBasis Basis    { get; }
		bool      HasLevel { get; }

		void BindLevel(LevelBehaviour level);
		void ClearLevel();

		bool IsInBounds(Vector2Int coordinates);
		bool CanOccupy(Vector2Int  coordinates);

		bool TryGetEntity(Vector2Int      coordinates, out INavCellEntity entity);
		bool TrySetEntity(Vector2Int      coordinates, INavCellEntity     entity);
		bool TryClearEntity(Vector2Int    coordinates, INavCellEntity     expectedEntity = null);
		bool TryMoveEntity(INavCellEntity entity,      Vector2Int         from, Vector2Int to);

		bool TryFindPath(Vector2Int                  start, Vector2Int goal, int[]               pathBuffer,     out NavPathResult result);
		bool TryFindPath(Vector2Int                  start, Vector2Int goal, Vector2Int[]        pathBuffer,     out NavPathResult result);
		bool TryFindPath<TWeightProvider>(Vector2Int start, Vector2Int goal, ref TWeightProvider weightProvider, Vector2Int[]      pathBuffer, out NavPathResult result) where TWeightProvider : struct, Navigation.Pathfinding.Providers.INavTraversalCostProvider;
	}
}