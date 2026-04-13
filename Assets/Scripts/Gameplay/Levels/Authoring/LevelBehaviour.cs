using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using TriInspector;
using UnityEngine;

namespace Gameplay.Levels.Authoring
{
	[ExecuteAlways]
	public sealed partial class LevelBehaviour : MonoBehaviour
	{
		[field: SerializeField, Required] public NavGrid     NavGrid     { get; private set; }
		[field: SerializeField, Required] public PlayerStart PlayerStart { get; private set; }
		[field: SerializeField]           public Transform   PropsRoot   { get; private set; }
		[field: SerializeField]           public Transform   TilesRoot   { get; private set; }

		public void Initialize()
		{
			RebuildRuntimeState(resetRuntimeState: true);
		}

		public TileBehaviour[] GetTileBehaviours()
		{
			return GetComponentsInChildren<TileBehaviour>(true);
		}

		public TileFloor[] GetTiles()
		{
			Transform tileRoot = TilesRoot != null ? TilesRoot : transform;
			return tileRoot.GetComponentsInChildren<TileFloor>(true);
		}

		public bool TryGetTile(Vector2Int gridPosition, out TileFloor tile)
		{
			TileFloor[] tiles = GetTiles();
			for (int i = 0; i < tiles.Length; i++) {
				TileFloor candidate = tiles[i];
				if (candidate != null && candidate.GridPosition == gridPosition) {
					tile = candidate;
					return true;
				}
			}

			tile = null;
			return false;
		}

		public bool TryGetTileBehaviourAt(Vector2Int gridPosition, out TileBehaviour tileBehaviour)
		{
			TileBehaviour[] tileBehaviours = GetTileBehaviours();
			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour candidate = tileBehaviours[i];
				if (candidate != null && candidate.GridPosition == gridPosition) {
					tileBehaviour = candidate;
					return true;
				}
			}

			tileBehaviour = null;
			return false;
		}

		private void Awake()
		{
			if (!Application.isPlaying) {
				return;
			}

			Initialize();
		}

		// Runtime and editor preview both rely on the same grid rebuild path so that
		// authoring state and in-game state are resolved identically.
		private void RebuildRuntimeState(bool resetRuntimeState)
		{
			if (NavGrid == null) {
				return;
			}

			NavGrid.RebuildGrid();
			NavGrid.ClearEntities();
			SynchronizePlayerStart();
			SynchronizeTileBehaviours(resetRuntimeState);
		}

		private void SynchronizePlayerStart()
		{
			if (PlayerStart == null) {
				return;
			}

			PlayerStart.SyncToGrid(NavGrid);
		}

		private void SynchronizeTileBehaviours(bool resetRuntimeState)
		{
			TileBehaviour[] tileBehaviours = GetTileBehaviours();

			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour == null) {
					continue;
				}

				if (resetRuntimeState) {
					// Preview sync and level load should both restore tiles to their
					// authored baseline before re-registering them into the grid.
					tileBehaviour.ResetRuntimeState();
				}

				if (!tileBehaviour.SyncToGrid(NavGrid)) {
					continue;
				}

				NavGrid.TrySetEntity(tileBehaviour.GridPosition, tileBehaviour.IsAlive ? tileBehaviour : null);
			}
		}
	}
}
