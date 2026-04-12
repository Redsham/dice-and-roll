using System.Collections.Generic;
using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using TriInspector;
using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	[ExecuteAlways]
	public sealed class LevelBehaviour : MonoBehaviour
	{
		[field: SerializeField, Required] public NavGrid     NavGrid     { get; private set; }
		[field: SerializeField, Required] public PlayerStart PlayerStart { get; private set; }
		[field: SerializeField]           public Transform   PropsRoot   { get; private set; }
		[field: SerializeField]           public Transform   TilesRoot   { get; private set; }

		private int m_LastEditorNodeCount = -1;
		private int m_LastEditorTileCount = -1;
		private int m_LastGridWidth       = -1;
		private int m_LastGridHeight      = -1;

		public void Initialize()
		{
			SyncLevel(resetRuntimeState: true);
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

		public void PreviewNodesToGrid()
		{
			SyncLevel(resetRuntimeState: true);
		}

		public void RebuildTiles()
		{
			SyncLevel(resetRuntimeState: false);
		}

		public bool PaintTile(Vector2Int gridPosition, GameObject tilePrefab)
		{
			if (!TryGetTile(gridPosition, out TileFloor tile) || tile == null) {
				return false;
			}

			if (tilePrefab == null) {
				return false;
			}

			TileFloor prefabTile = tilePrefab.GetComponent<TileFloor>();
			if (prefabTile == null) {
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
			if (tilePrefab == null) {
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

			TileBehaviour prefabBehaviour = nodePrefab.GetComponent<TileBehaviour>();
			if (prefabBehaviour == null) {
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
			if (nodePrefab == null) {
				return;
			}

			for (int y = 0; y < NavGrid.Height; y++) {
				for (int x = 0; x < NavGrid.Width; x++) {
					PaintNode(new Vector2Int(x, y), nodePrefab);
				}
			}
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

		private void Awake()
		{
			if (!Application.isPlaying) {
				return;
			}

			Initialize();
		}

		private void OnEnable()
		{
			if (Application.isPlaying || !CanRunEditorSync()) {
				return;
			}

			SyncLevel(resetRuntimeState: true);
		}

		private void OnValidate()
		{
			if (Application.isPlaying || !CanRunEditorSync()) {
				return;
			}

			SyncLevel(resetRuntimeState: true);
		}

		private void Update()
		{
			if (Application.isPlaying || !CanRunEditorSync() || !HasEditorChanges()) {
				return;
			}

			SyncLevel(resetRuntimeState: true);
		}

		private void SyncLevel(bool resetRuntimeState)
		{
			if (!Application.isPlaying && !CanRunEditorSync()) {
				return;
			}

			if (NavGrid == null) {
				ClearNodeValidation("NavGrid is not assigned on LevelBehaviour.");
				ClearPlayerStartValidation("NavGrid is not assigned on LevelBehaviour.");
				return;
			}

			Dictionary<Vector2Int, TileFloor> tiles = EnsureTileHierarchy();

			NavGrid.RebuildGrid();
			NavGrid.ClearEntities();

			SyncPlayerStart();
			SyncTileBehaviours(tiles, resetRuntimeState);
			ResetTrackedTransforms();
			RememberGridShape();
			MarkLevelChanged();
		}

		private void SyncTileBehaviours(Dictionary<Vector2Int, TileFloor> tiles, bool resetRuntimeState)
		{
			TileBehaviour[] tileBehaviours = GetTileBehaviours();
			m_LastEditorNodeCount = tileBehaviours.Length;

			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour == null) {
					continue;
				}

				if (!Application.isPlaying) {
					AttachTileBehaviourToTile(tileBehaviour, tiles);
				}

				if (resetRuntimeState) {
					tileBehaviour.ResetRuntimeState();
				}

				if (!tileBehaviour.SyncToGrid(NavGrid)) {
					MarkTileBehaviourChanged(tileBehaviour);
					continue;
				}

				NavGrid.TrySetEntity(tileBehaviour.GridPosition, tileBehaviour.IsAlive ? tileBehaviour : null);
				MarkTileBehaviourChanged(tileBehaviour);
			}
		}

		private void SyncPlayerStart()
		{
			if (PlayerStart == null) {
				return;
			}

			PlayerStart.SyncToGrid(NavGrid);
			MarkPlayerStartChanged();
		}

		private void AttachTileBehaviourToTile(TileBehaviour tileBehaviour, IReadOnlyDictionary<Vector2Int, TileFloor> tiles)
		{
			Vector2Int targetCell = NavGrid.GetCellCoordinates(tileBehaviour.transform.position, tileBehaviour.Alignment);
			if (!tiles.TryGetValue(targetCell, out TileFloor tile) || tile == null) {
				return;
			}

			Transform tileBehaviourTransform = tileBehaviour.transform;
			if (tileBehaviourTransform.parent != tile.transform) {
#if UNITY_EDITOR
				if (CanReparentEditorTransform(tileBehaviourTransform) && CanReparentEditorTransform(tile.transform)) {
					UnityEditor.Undo.SetTransformParent(tileBehaviourTransform, tile.transform, "Parent TileBehaviour To Tile");
				}
#else
				tileBehaviourTransform.SetParent(tile.transform, true);
#endif
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

#if UNITY_EDITOR
			GameObject rootObject = new("Tiles");
			UnityEditor.Undo.RegisterCreatedObjectUndo(rootObject, "Create Tiles Root");
			rootObject.transform.SetParent(NavGrid.transform, false);
			TilesRoot = rootObject.transform;
			MarkLevelChanged();
			return TilesRoot;
#else
			GameObject rootObject = new("Tiles");
			rootObject.transform.SetParent(NavGrid.transform, false);
			TilesRoot = rootObject.transform;
			return TilesRoot;
#endif
		}

		private TileFloor CreateTile(Transform tilesRoot, Vector2Int cell)
		{
			GameObject tileObject = new($"Tile_{cell.x}_{cell.y}");

#if UNITY_EDITOR
			UnityEditor.Undo.RegisterCreatedObjectUndo(tileObject, "Create Tile");
#endif

			tileObject.transform.SetParent(tilesRoot, false);
			TileFloor tile = tileObject.AddComponent<TileFloor>();
			tile.SetGridPosition(cell);
			MarkTileChanged(tile);
			return tile;
		}

		private void ReplaceTile(TileFloor currentTile, GameObject tilePrefab, Vector2Int gridPosition)
		{
			GameObject newTileObject = InstantiateEditorPrefab(tilePrefab);
			TileFloor   newTile      = newTileObject.GetComponent<TileFloor>();
			newTileObject.name = tilePrefab.name;

			Transform currentTransform = currentTile.transform;
			while (currentTransform.childCount > 0) {
				Transform child = currentTransform.GetChild(0);
#if UNITY_EDITOR
				UnityEditor.Undo.SetTransformParent(child, newTileObject.transform, "Reparent Tile Child");
#else
				child.SetParent(newTileObject.transform, true);
#endif
			}

			newTileObject.transform.SetParent(TilesRoot, false);
			newTile.SetGridPosition(gridPosition);
			AlignTile(newTile, gridPosition);
			DestroyTile(currentTile.gameObject);
		}

		private void ReplaceTileWithDefault(TileFloor currentTile, Vector2Int gridPosition)
		{
			GameObject defaultTileObject = new($"Tile_{gridPosition.x}_{gridPosition.y}");

#if UNITY_EDITOR
			UnityEditor.Undo.RegisterCreatedObjectUndo(defaultTileObject, "Clear Floor Tile");
#endif

			TileFloor defaultTile = defaultTileObject.AddComponent<TileFloor>();
			MoveTileChildren(currentTile.transform, defaultTileObject.transform);
			defaultTileObject.transform.SetParent(TilesRoot, false);
			defaultTile.SetGridPosition(gridPosition);
			AlignTile(defaultTile, gridPosition);
			DestroyTile(currentTile.gameObject);
		}

		private static void MoveTileChildren(Transform from, Transform to)
		{
			while (from.childCount > 0) {
				Transform child = from.GetChild(0);
#if UNITY_EDITOR
				UnityEditor.Undo.SetTransformParent(child, to, "Reparent Tile Child");
#else
				child.SetParent(to, true);
#endif
			}
		}

		private void AlignTile(TileFloor tile, Vector2Int cell)
		{
			tile.SetGridPosition(cell);

			if (tile.transform.parent != TilesRoot) {
#if UNITY_EDITOR
				if (CanReparentEditorTransform(tile.transform) && CanReparentEditorTransform(TilesRoot)) {
					UnityEditor.Undo.SetTransformParent(tile.transform, TilesRoot, "Parent Tile To TilesRoot");
				}
#else
				tile.transform.SetParent(TilesRoot, false);
#endif
			}

			tile.transform.position = tile.GetAlignedWorldPosition(NavGrid, cell);
			tile.transform.localRotation = Quaternion.identity;
			MarkTileChanged(tile);
		}

		private void DestroyTile(GameObject tileObject)
		{
			Transform tileTransform   = tileObject.transform;
			Transform fallbackParent = PropsRoot != null ? PropsRoot : transform;

			while (tileTransform.childCount > 0) {
				Transform child = tileTransform.GetChild(0);
#if UNITY_EDITOR
				UnityEditor.Undo.SetTransformParent(child, fallbackParent, "Detach Tile Child");
#else
				child.SetParent(fallbackParent, true);
#endif
			}

#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(tileObject);
#else
			Destroy(tileObject);
#endif
		}

		private void DestroyTileBehaviour(TileBehaviour tileBehaviour)
		{
			if (tileBehaviour == null) {
				return;
			}

#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(tileBehaviour.gameObject);
#else
			Destroy(tileBehaviour.gameObject);
#endif
		}

		private static GameObject InstantiateEditorPrefab(GameObject prefab)
		{
#if UNITY_EDITOR
			GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
			if (instance != null) {
				UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Instantiate Palette Prefab");
				return instance;
			}
#endif
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

			if (NavGrid.transform.hasChanged) {
				return true;
			}

			if (PropsRoot != null && PropsRoot.hasChanged) {
				return true;
			}

			if (TilesRoot != null && TilesRoot.hasChanged) {
				return true;
			}

			if (transform.hasChanged) {
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
#if UNITY_EDITOR
			return !UnityEditor.EditorUtility.IsPersistent(this) && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
#else
			return true;
#endif
		}

#if UNITY_EDITOR
		private static bool CanReparentEditorTransform(Transform target)
		{
			return target != null
				&& !UnityEditor.EditorUtility.IsPersistent(target)
				&& !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(target.gameObject);
		}
#endif

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

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private static void MarkTileBehaviourChanged(TileBehaviour tileBehaviour)
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(tileBehaviour);
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private static void MarkTileChanged(TileFloor tile)
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(tile);
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void MarkPlayerStartChanged()
		{
#if UNITY_EDITOR
			if (PlayerStart != null) {
				UnityEditor.EditorUtility.SetDirty(PlayerStart);
			}
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void MarkLevelChanged()
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
			if (NavGrid != null) {
				UnityEditor.EditorUtility.SetDirty(NavGrid);
			}
#endif
		}
	}
}
