#if UNITY_EDITOR
using System.Collections.Generic;
using Gameplay.Nodes.Authoring;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Levels.Authoring
{
	public sealed partial class LevelBehaviour
	{
		private int m_LastEditorNodeCount = -1;
		private int m_LastEditorTileCount = -1;
		private int m_LastGridWidth       = -1;
		private int m_LastGridHeight      = -1;

		public void PreviewNodesToGrid()
		{
			SynchronizeEditorState(resetRuntimeState: true);
		}

		public void RebuildTiles()
		{
			SynchronizeEditorState(resetRuntimeState: false);
		}

		public bool PaintTile(Vector2Int gridPosition, GameObject tilePrefab)
		{
			if (!TryGetTile(gridPosition, out TileFloor tile) || tile == null) {
				return false;
			}

			if (tilePrefab == null || tilePrefab.GetComponent<TileFloor>() == null) {
				return false;
			}

			ReplaceTile(tile, tilePrefab, gridPosition);
			return true;
		}

		public bool ClearTileAt(Vector2Int gridPosition)
		{
			if (!TryGetTile(gridPosition, out TileFloor tile) || tile == null) {
				return false;
			}

			ReplaceTileWithDefault(tile, gridPosition);
			return true;
		}

		public void FillTiles(GameObject tilePrefab)
		{
			if (tilePrefab == null || NavGrid == null) {
				return;
			}

			for (int y = 0; y < NavGrid.Height; y++) {
				for (int x = 0; x < NavGrid.Width; x++) {
					PaintTile(new Vector2Int(x, y), tilePrefab);
				}
			}
		}

		public bool ClearNodeAt(Vector2Int gridPosition)
		{
			if (!TryGetTileBehaviourAt(gridPosition, out TileBehaviour tileBehaviour) || tileBehaviour == null) {
				return false;
			}

			DestroyTileBehaviour(tileBehaviour);
			return true;
		}

		public bool PaintNode(Vector2Int gridPosition, GameObject nodePrefab)
		{
			if (nodePrefab == null) {
				return false;
			}

			if (!TryGetTile(gridPosition, out TileFloor tile) || tile == null) {
				return false;
			}

			if (nodePrefab.GetComponent<TileBehaviour>() == null) {
				return false;
			}

			if (TryGetTileBehaviourAt(gridPosition, out TileBehaviour existingTileBehaviour) && existingTileBehaviour != null) {
				DestroyTileBehaviour(existingTileBehaviour);
			}

			GameObject nodeObject = InstantiateEditorPrefab(nodePrefab);
			TileBehaviour tileBehaviour = nodeObject.GetComponent<TileBehaviour>();
			if (tileBehaviour == null) {
				DestroyImmediate(nodeObject);
				return false;
			}

			nodeObject.name = nodePrefab.name;
			nodeObject.transform.SetParent(tile.transform, false);
			tileBehaviour.SetGridPosition(gridPosition);
			nodeObject.transform.position = tileBehaviour.GetAlignedWorldPosition(NavGrid, gridPosition);
			MarkTileBehaviourChanged(tileBehaviour);
			return true;
		}

		public void FillTileBehaviours(GameObject nodePrefab)
		{
			if (nodePrefab == null || NavGrid == null) {
				return;
			}

			for (int y = 0; y < NavGrid.Height; y++) {
				for (int x = 0; x < NavGrid.Width; x++) {
					PaintNode(new Vector2Int(x, y), nodePrefab);
				}
			}
		}

		public bool RotateTile(Vector2Int gridPosition)
		{
			if (!TryGetTile(gridPosition, out TileFloor tile) || tile == null) {
				return false;
			}

			tile.transform.Rotate(Vector3.up, 90f, Space.Self);
			MarkTileChanged(tile);
			return true;
		}

		public bool RotateTileBehaviour(Vector2Int gridPosition)
		{
			if (!TryGetTileBehaviourAt(gridPosition, out TileBehaviour tileBehaviour) || tileBehaviour == null) {
				return false;
			}

			tileBehaviour.transform.Rotate(Vector3.up, 90f, Space.Self);
			MarkTileBehaviourChanged(tileBehaviour);
			return true;
		}

		private void OnEnable()
		{
			if (Application.isPlaying || !CanRunEditorSync()) {
				return;
			}

			SynchronizeEditorState(resetRuntimeState: true);
		}

		private void OnValidate()
		{
			if (Application.isPlaying || !CanRunEditorSync()) {
				return;
			}

			SynchronizeEditorState(resetRuntimeState: true);
		}

		private void Update()
		{
			if (Application.isPlaying || !CanRunEditorSync() || !HasEditorChanges()) {
				return;
			}

			SynchronizeEditorState(resetRuntimeState: true);
		}

		// Editor sync is responsible for keeping the authored hierarchy valid:
		// generated tiles, parenting, warnings and dirty-state tracking.
		private void SynchronizeEditorState(bool resetRuntimeState)
		{
			if (!CanRunEditorSync()) {
				return;
			}

			if (NavGrid == null) {
				ClearNodeValidation("NavGrid is not assigned on LevelBehaviour.");
				ClearPlayerStartValidation("NavGrid is not assigned on LevelBehaviour.");
				return;
			}

			Dictionary<Vector2Int, TileFloor> tiles = EnsureTileHierarchy();
			AttachTileBehavioursToTiles(tiles);
			RebuildRuntimeState(resetRuntimeState);
			RefreshEditorTracking();
		}

		private void AttachTileBehavioursToTiles(IReadOnlyDictionary<Vector2Int, TileFloor> tiles)
		{
			TileBehaviour[] tileBehaviours = GetTileBehaviours();
			m_LastEditorNodeCount = tileBehaviours.Length;

			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour == null) {
					continue;
				}

				Vector2Int targetCell = NavGrid.GetCellCoordinates(tileBehaviour.transform.position, tileBehaviour.Pivot);
				if (!tiles.TryGetValue(targetCell, out TileFloor tile) || tile == null) {
					continue;
				}

				Transform tileBehaviourTransform = tileBehaviour.transform;
				if (tileBehaviourTransform.parent == tile.transform) {
					continue;
				}

				if (CanReparentEditorTransform(tileBehaviourTransform) && CanReparentEditorTransform(tile.transform)) {
					Undo.SetTransformParent(tileBehaviourTransform, tile.transform, "Parent TileBehaviour To Tile");
				}
			}
		}

		private Dictionary<Vector2Int, TileFloor> EnsureTileHierarchy()
		{
			Transform tilesRoot = EnsureTilesRoot();
			Dictionary<Vector2Int, TileFloor> tilesByPosition = new();
			TileFloor[] existingTiles = tilesRoot.GetComponentsInChildren<TileFloor>(true);

			for (int i = 0; i < existingTiles.Length; i++) {
				TileFloor tile = existingTiles[i];
				if (tile == null) {
					continue;
				}

				tilesByPosition[tile.GridPosition] = tile;
			}

			for (int y = 0; y < NavGrid.Height; y++) {
				for (int x = 0; x < NavGrid.Width; x++) {
					Vector2Int cell = new(x, y);
					if (!tilesByPosition.TryGetValue(cell, out TileFloor tile) || tile == null) {
						tile = CreateTile(tilesRoot, cell);
						tilesByPosition[cell] = tile;
					}

					AlignTile(tile, cell);
				}
			}

			for (int i = 0; i < existingTiles.Length; i++) {
				TileFloor tile = existingTiles[i];
				if (tile == null) {
					continue;
				}

				if (!NavGrid.IsInBounds(tile.GridPosition)) {
					DestroyTile(tile.gameObject);
				}
			}

			m_LastEditorTileCount = NavGrid.NodeCount;
			return tilesByPosition;
		}

		private Transform EnsureTilesRoot()
		{
			if (TilesRoot != null) {
				return TilesRoot;
			}

			GameObject rootObject = new("Tiles");
			Undo.RegisterCreatedObjectUndo(rootObject, "Create Tiles Root");
			rootObject.transform.SetParent(NavGrid.transform, false);
			TilesRoot = rootObject.transform;
			MarkLevelChanged();
			return TilesRoot;
		}

		private TileFloor CreateTile(Transform tilesRoot, Vector2Int cell)
		{
			GameObject tileObject = new($"Tile_{cell.x}_{cell.y}");
			Undo.RegisterCreatedObjectUndo(tileObject, "Create Tile");

			tileObject.transform.SetParent(tilesRoot, false);
			TileFloor tile = tileObject.AddComponent<TileFloor>();
			tile.SetGridPosition(cell);
			MarkTileChanged(tile);
			return tile;
		}

		private void ReplaceTile(TileFloor currentTile, GameObject tilePrefab, Vector2Int gridPosition)
		{
			GameObject newTileObject = InstantiateEditorPrefab(tilePrefab);
			TileFloor newTile = newTileObject.GetComponent<TileFloor>();
			newTileObject.name = tilePrefab.name;

			newTileObject.transform.SetParent(TilesRoot, false);
			newTile.SetGridPosition(gridPosition);
			AlignTile(newTile, gridPosition);
			MoveTileChildrenToReplacement(currentTile.transform, newTileObject.transform);
			DestroyTile(currentTile.gameObject);
		}

		private void ReplaceTileWithDefault(TileFloor currentTile, Vector2Int gridPosition)
		{
			GameObject defaultTileObject = new($"Tile_{gridPosition.x}_{gridPosition.y}");
			Undo.RegisterCreatedObjectUndo(defaultTileObject, "Clear Floor Tile");

			TileFloor defaultTile = defaultTileObject.AddComponent<TileFloor>();
			defaultTileObject.transform.SetParent(TilesRoot, false);
			defaultTile.SetGridPosition(gridPosition);
			AlignTile(defaultTile, gridPosition);
			MoveTileChildrenToReplacement(currentTile.transform, defaultTileObject.transform);
			DestroyTile(currentTile.gameObject);
		}

		private static void MoveTileChildrenToReplacement(Transform from, Transform to)
		{
			while (from.childCount > 0) {
				Transform child = from.GetChild(0);
				Vector3 localPosition = child.localPosition;
				Quaternion localRotation = child.localRotation;
				Vector3 localScale = child.localScale;
				Undo.SetTransformParent(child, to, "Reparent Tile Child");
				child.localPosition = localPosition;
				child.localRotation = localRotation;
				child.localScale = localScale;
			}
		}

		private void AlignTile(TileFloor tile, Vector2Int cell)
		{
			tile.SetGridPosition(cell);

			if (tile.transform.parent != TilesRoot && CanReparentEditorTransform(tile.transform) && CanReparentEditorTransform(TilesRoot)) {
				Undo.SetTransformParent(tile.transform, TilesRoot, "Parent Tile To TilesRoot");
			}

			tile.transform.position = tile.GetAlignedWorldPosition(NavGrid, cell);
			tile.transform.localRotation = Quaternion.identity;
			MarkTileChanged(tile);
		}

		private void DestroyTile(GameObject tileObject)
		{
			Transform tileTransform = tileObject.transform;
			Transform fallbackParent = PropsRoot != null ? PropsRoot : transform;

			while (tileTransform.childCount > 0) {
				Transform child = tileTransform.GetChild(0);
				Undo.SetTransformParent(child, fallbackParent, "Detach Tile Child");
			}

			Undo.DestroyObjectImmediate(tileObject);
		}

		private static void DestroyTileBehaviour(TileBehaviour tileBehaviour)
		{
			if (tileBehaviour != null) {
				Undo.DestroyObjectImmediate(tileBehaviour.gameObject);
			}
		}

		private static GameObject InstantiateEditorPrefab(GameObject prefab)
		{
			GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			if (instance != null) {
				Undo.RegisterCreatedObjectUndo(instance, "Instantiate Palette Prefab");
				return instance;
			}

			return Instantiate(prefab);
		}

		private void ClearNodeValidation(string warning)
		{
			TileBehaviour[] tileBehaviours = GetTileBehaviours();
			m_LastEditorNodeCount = tileBehaviours.Length;

			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour == null) {
					continue;
				}

				tileBehaviour.ClearGridValidation(warning);
				MarkTileBehaviourChanged(tileBehaviour);
			}
		}

		private void ClearPlayerStartValidation(string warning)
		{
			if (PlayerStart == null) {
				return;
			}

			PlayerStart.ClearGridValidation(warning);
			MarkPlayerStartChanged();
		}

		private bool HasEditorChanges()
		{
			if (NavGrid == null) {
				return false;
			}

			if (m_LastGridWidth != NavGrid.Width || m_LastGridHeight != NavGrid.Height) {
				return true;
			}

			TileBehaviour[] tileBehaviours = GetTileBehaviours();
			if (m_LastEditorNodeCount != tileBehaviours.Length) {
				return true;
			}

			TileFloor[] tiles = GetTiles();
			if (m_LastEditorTileCount != NavGrid.NodeCount || tiles.Length != NavGrid.NodeCount) {
				return true;
			}

			if (NavGrid.transform.hasChanged || (PropsRoot != null && PropsRoot.hasChanged) || (TilesRoot != null && TilesRoot.hasChanged) || transform.hasChanged) {
				return true;
			}

			if (PlayerStart != null && PlayerStart.transform.hasChanged) {
				return true;
			}

			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour != null && tileBehaviour.transform.hasChanged) {
					return true;
				}
			}

			for (int i = 0; i < tiles.Length; i++) {
				TileFloor tile = tiles[i];
				if (tile != null && tile.transform.hasChanged) {
					return true;
				}
			}

			return false;
		}

		private bool CanRunEditorSync()
		{
			return !EditorUtility.IsPersistent(this) && !PrefabUtility.IsPartOfPrefabAsset(gameObject);
		}

		private static bool CanReparentEditorTransform(Transform target)
		{
			return target != null
			    && !EditorUtility.IsPersistent(target)
			    && !PrefabUtility.IsPartOfPrefabAsset(target.gameObject);
		}

		private void RefreshEditorTracking()
		{
			ResetTrackedTransforms();
			RememberGridShape();
			MarkLevelChanged();
		}

		private void ResetTrackedTransforms()
		{
			transform.hasChanged = false;

			if (NavGrid != null) {
				NavGrid.transform.hasChanged = false;
			}

			if (PropsRoot != null) {
				PropsRoot.hasChanged = false;
			}

			if (TilesRoot != null) {
				TilesRoot.hasChanged = false;
			}

			if (PlayerStart != null) {
				PlayerStart.transform.hasChanged = false;
			}

			TileBehaviour[] tileBehaviours = GetTileBehaviours();
			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour != null) {
					tileBehaviour.transform.hasChanged = false;
				}
			}

			TileFloor[] tiles = GetTiles();
			for (int i = 0; i < tiles.Length; i++) {
				TileFloor tile = tiles[i];
				if (tile != null) {
					tile.transform.hasChanged = false;
				}
			}
		}

		private void RememberGridShape()
		{
			m_LastGridWidth  = NavGrid.Width;
			m_LastGridHeight = NavGrid.Height;
		}

		private static void MarkTileBehaviourChanged(TileBehaviour tileBehaviour)
		{
			EditorUtility.SetDirty(tileBehaviour);
		}

		private static void MarkTileChanged(TileFloor tile)
		{
			EditorUtility.SetDirty(tile);
		}

		private void MarkPlayerStartChanged()
		{
			if (PlayerStart != null) {
				EditorUtility.SetDirty(PlayerStart);
			}
		}

		private void MarkLevelChanged()
		{
			EditorUtility.SetDirty(this);
			if (NavGrid != null) {
				EditorUtility.SetDirty(NavGrid);
			}
		}
	}
}
#endif
