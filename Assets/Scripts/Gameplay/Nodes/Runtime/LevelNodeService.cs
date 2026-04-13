using System.Collections.Generic;
using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using Gameplay.Nodes.Contracts;
using UnityEngine;


namespace Gameplay.Nodes.Runtime
{
	public sealed class LevelNodeService : ILevelNodeService
	{
		private readonly Dictionary<Vector2Int, TileBehaviour> m_Tiles = new();

		private NavGrid m_CurrentGrid;

		public void BindLevel(NavGrid navGrid, TileBehaviour[] tiles)
		{
			m_CurrentGrid = navGrid;
			m_Tiles.Clear();

			for (int i = 0; i < tiles.Length; i++) {
				TileBehaviour tile = tiles[i];
				tile.ResetRuntimeState();
				tile.BindToGrid(navGrid);
				m_Tiles[tile.GridPosition] = tile;
			}
		}

		public void ClearLevel()
		{
			foreach (TileBehaviour tile in m_Tiles.Values) {
				tile?.ClearBoundGrid();
			}

			m_CurrentGrid = null;
			m_Tiles.Clear();
		}

		public bool TryGetTile(Vector2Int cell, out TileBehaviour tile)
		{
			return m_Tiles.TryGetValue(cell, out tile);
		}

		public void NotifyActorEntered(Vector2Int cell, GameObject actor)
		{
			if (!m_Tiles.TryGetValue(cell, out TileBehaviour tile)) {
				return;
			}

			if (tile is INodeActorEnterHandler handler) {
				handler.OnActorEnter(new(actor, cell));
				SyncTile(cell, tile);
			}
		}

		public void NotifyActorLeft(Vector2Int cell, GameObject actor)
		{
			if (!m_Tiles.TryGetValue(cell, out TileBehaviour tile)) {
				return;
			}

			if (tile is INodeActorLeaveHandler handler) {
				handler.OnActorLeave(new(actor, cell));
				SyncTile(cell, tile);
			}
		}

		private void SyncTile(Vector2Int cell, TileBehaviour tile)
		{
			if (m_CurrentGrid == null) {
				return;
			}

			if (tile != null && tile.IsAlive) {
				m_CurrentGrid.TrySetEntity(cell, tile);
				return;
			}

			m_CurrentGrid.TryClearEntity(cell, tile);
		}
	}
}
