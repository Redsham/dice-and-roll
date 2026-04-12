using System;
using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using Gameplay.Navigation.Pathfinding;
using Gameplay.Navigation.Pathfinding.Providers;
using UnityEngine;


namespace Gameplay.World.Runtime
{
	public sealed class NavigationService : INavigationService, INavEntityService
	{
		private const float CELL_SIZE = 1.0f;

		private LevelBehaviour m_CurrentLevel;

		public NavGrid Grid => RequireGrid();
		public bool HasLevel => m_CurrentLevel != null;

		public GridBasis Basis
		{
			get
			{
				NavGrid navGrid = RequireGrid();
				return new(
				           navGrid.transform.position,
				           navGrid.transform.right.normalized,
				           navGrid.transform.forward.normalized,
				           navGrid.transform.up.normalized,
				           CELL_SIZE
				          );
			}
		}

		public void BindLevel(LevelBehaviour level)
		{
			m_CurrentLevel = level;
			EnsureGridReady();
		}

		public void ClearLevel()
		{
			m_CurrentLevel = null;
		}

		public bool IsInBounds(Vector2Int coordinates)
		{
			EnsureGridReady();
			return RequireGrid().IsInBounds(coordinates);
		}

		public bool CanOccupy(Vector2Int coordinates)
		{
			EnsureGridReady();
			NavGrid navGrid = RequireGrid();

			if (!navGrid.IsInBounds(coordinates)) {
				return false;
			}

			int index = navGrid.ToIndex(coordinates);
			return navGrid.Nodes[index].CanOccupy;
		}

		public bool TryGetEntity(Vector2Int coordinates, out INavCellEntity entity)
		{
			EnsureGridReady();
			return RequireGrid().TryGetEntity(coordinates, out entity);
		}

		public bool TrySetEntity(Vector2Int coordinates, INavCellEntity entity)
		{
			EnsureGridReady();
			return RequireGrid().TrySetEntity(coordinates, entity);
		}

		public bool TryClearEntity(Vector2Int coordinates, INavCellEntity expectedEntity = null)
		{
			EnsureGridReady();
			return RequireGrid().TryClearEntity(coordinates, expectedEntity);
		}

		public bool TryMoveEntity(INavCellEntity entity, Vector2Int from, Vector2Int to)
		{
			EnsureGridReady();
			return RequireGrid().TryMoveEntity(entity, from, to);
		}

		public bool TryFindPath(Vector2Int start, Vector2Int goal, int[] pathBuffer, out NavPathResult result)
		{
			EnsureGridReady();
			return RequireGrid().TryFindPath(start, goal, pathBuffer, out result);
		}

		public bool TryFindPath(Vector2Int start, Vector2Int goal, Vector2Int[] pathBuffer, out NavPathResult result)
		{
			EnsureGridReady();
			return RequireGrid().TryFindPath(start, goal, pathBuffer, out result);
		}

		public bool TryFindPath<TWeightProvider>(
			Vector2Int          start,
			Vector2Int          goal,
			ref TWeightProvider weightProvider,
			Vector2Int[]        pathBuffer,
			out NavPathResult   result
		)
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			EnsureGridReady();
			return RequireGrid().TryFindPath(start, goal, ref weightProvider, pathBuffer, out result);
		}

		private void EnsureGridReady()
		{
			NavGrid navGrid = RequireGrid();
			if (navGrid.Nodes.Data != null && navGrid.Nodes.Data.Length == navGrid.NodeCount) {
				return;
			}

			navGrid.RebuildGrid();
		}

		private NavGrid RequireGrid()
		{
			if (m_CurrentLevel == null) {
				throw new InvalidOperationException("NavigationService requires an active level before use.");
			}

			return m_CurrentLevel.NavGrid;
		}
	}
}
