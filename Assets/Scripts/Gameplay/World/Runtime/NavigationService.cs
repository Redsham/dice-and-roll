using System;
using Gameplay.Actors.Runtime;
using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using Gameplay.Navigation.Pathfinding;
using UnityEngine;


namespace Gameplay.World.Runtime
{
	public sealed class NavigationService : INavigationService
	{
		private const float CellSize = 1.0f;

		private readonly IGridActorRegistry m_ActorRegistry;
		private          LevelBehaviour     m_CurrentLevel;

		public NavigationService(IGridActorRegistry actorRegistry)
		{
			m_ActorRegistry = actorRegistry;
		}

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
				           CellSize
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

		public bool CanOccupy(Vector2Int coordinates)
		{
			EnsureGridReady();
			NavGrid navGrid = RequireGrid();

			if (!navGrid.IsInBounds(coordinates)) {
				return false;
			}

			int index = navGrid.ToIndex(coordinates);
			return navGrid.Nodes[index].CanOccupy && !m_ActorRegistry.IsOccupied(coordinates);
		}

		public bool TryGetOccupancy(Vector2Int coordinates, out NavCellOccupancy occupancy)
		{
			EnsureGridReady();
			return RequireGrid().TryGetOccupancy(coordinates, out occupancy);
		}

		public bool TryFindPath(Vector2Int start, Vector2Int goal, int[] pathBuffer, out NavPathResult result)
		{
			EnsureGridReady();
			return RequireGrid().TryFindPath(start, goal, pathBuffer, out result);
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