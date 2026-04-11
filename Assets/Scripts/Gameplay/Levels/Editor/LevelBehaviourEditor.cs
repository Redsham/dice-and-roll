using System.Collections.Generic;
using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Levels.Editor
{
	[CustomEditor(typeof(LevelBehaviour))]
	public sealed class LevelBehaviourEditor : UnityEditor.Editor
	{
		private static readonly Color PaintPreviewColor = new(0.22f, 0.78f, 0.32f, 0.2f);
		private static Vector2Int? m_LastDraggedCell;

		public override void OnInspectorGUI()
		{
			LevelBehaviour level = (LevelBehaviour)target;

			DrawDefaultInspector();
			DrawWarnings(level);

			EditorGUILayout.Space();
			if (GUILayout.Button("Open Palette")) {
				LevelPaletteWindow.Open();
			}

			if (GUILayout.Button("Sync Level To Grid")) {
				SyncLevel(level);
			}

			if (GUILayout.Button("Rebuild Tiles")) {
				RebuildTiles(level);
			}
		}

		private void OnSceneGUI()
		{
			if (!LevelPaletteState.IsEnabled) {
				m_LastDraggedCell = null;
				return;
			}

			LevelBehaviour level = (LevelBehaviour)target;
			if (level == null || level.NavGrid == null) {
				return;
			}

			Event sceneEvent = Event.current;
			if (!TryGetHoveredCell(level.NavGrid, sceneEvent.mousePosition, out Vector2Int hoveredCell)) {
				if (sceneEvent.type == EventType.MouseUp) {
					m_LastDraggedCell = null;
				}

				return;
			}

			DrawHoveredCell(level.NavGrid, hoveredCell);

			if (sceneEvent.type == EventType.Layout) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			}

			if (sceneEvent.type == EventType.MouseDown && sceneEvent.button == 0 && !sceneEvent.alt) {
				m_LastDraggedCell = hoveredCell;
				ApplyTool(level, hoveredCell);
				SceneView.RepaintAll();
				sceneEvent.Use();
				return;
			}

			if (sceneEvent.type == EventType.MouseDrag && sceneEvent.button == 0 && !sceneEvent.alt) {
				if (m_LastDraggedCell != hoveredCell) {
					m_LastDraggedCell = hoveredCell;
					ApplyTool(level, hoveredCell);
					SceneView.RepaintAll();
				}

				sceneEvent.Use();
				return;
			}

			if (sceneEvent.type == EventType.MouseUp && sceneEvent.button == 0) {
				m_LastDraggedCell = null;
			}
		}

		private static void ApplyTool(LevelBehaviour level, Vector2Int cell)
		{
			switch (LevelPaletteState.ActiveTool) {
				case PaletteTool.Paint:
					PaintCell(level, cell);
					break;
				case PaletteTool.Fill:
					FillLayer(level, cell);
					break;
				case PaletteTool.Erase:
					EraseCell(level, cell);
					break;
				case PaletteTool.Rotate:
					RotateCell(level, cell);
					break;
			}

			EditorUtility.SetDirty(level);
			if (level.NavGrid != null) {
				EditorUtility.SetDirty(level.NavGrid);
			}

			level.PreviewNodesToGrid();
		}

		private static void PaintCell(LevelBehaviour level, Vector2Int cell)
		{
			switch (LevelPaletteState.ActiveLayer) {
				case PaletteLayer.Floor:
					if (level.TryGetTile(cell, out TileFloor tile) && tile != null && LevelPaletteState.SelectedFloorPrefab != null) {
						Undo.RecordObject(tile, "Paint Floor Tile");
						level.PaintTile(cell, LevelPaletteState.SelectedFloorPrefab);
					}
					break;
				case PaletteLayer.Object:
					if (LevelPaletteState.SelectedObjectEntry == null || LevelPaletteState.SelectedObjectEntry.Prefab == null) {
						level.ClearNodeAt(cell);
					} else {
						level.PaintNode(cell, LevelPaletteState.SelectedObjectEntry.Prefab);
					}
					break;
			}
		}

		private static void FillLayer(LevelBehaviour level, Vector2Int startCell)
		{
			switch (LevelPaletteState.ActiveLayer) {
				case PaletteLayer.Floor:
					if (LevelPaletteState.SelectedFloorPrefab != null) {
						FloodFillFloor(level, startCell, LevelPaletteState.SelectedFloorPrefab);
					}
					break;
				case PaletteLayer.Object:
					if (LevelPaletteState.SelectedObjectEntry != null && LevelPaletteState.SelectedObjectEntry.Prefab != null) {
						FloodFillObjects(level, startCell, LevelPaletteState.SelectedObjectEntry.Prefab);
					}
					break;
			}
		}

		private static void EraseCell(LevelBehaviour level, Vector2Int cell)
		{
			switch (LevelPaletteState.ActiveLayer) {
				case PaletteLayer.Floor:
					level.ClearTileAt(cell);
					break;
				case PaletteLayer.Object:
					level.ClearNodeAt(cell);
					break;
			}
		}

		private static void RotateCell(LevelBehaviour level, Vector2Int cell)
		{
			switch (LevelPaletteState.ActiveLayer) {
				case PaletteLayer.Floor:
					if (level.TryGetTile(cell, out TileFloor tile) && tile != null) {
						Undo.RecordObject(tile.transform, "Rotate Tile");
						level.RotateTile(cell);
					}
					break;
				case PaletteLayer.Object:
					if (level.TryGetTileBehaviourAt(cell, out TileBehaviour tileBehaviour) && tileBehaviour != null) {
						Undo.RecordObject(tileBehaviour.transform, "Rotate TileBehaviour");
						level.RotateTileBehaviour(cell);
					}
					break;
			}
		}

		private static void DrawWarnings(LevelBehaviour level)
		{
			if (level.NavGrid == null) {
				EditorGUILayout.HelpBox("LevelBehaviour requires a NavGrid reference.", MessageType.Error);
				return;
			}

			TileBehaviour[] tileBehaviours   = level.GetTileBehaviours();
			int             outOfBounds      = 0;
			int             detachedFromTile = 0;

			for (int i = 0; i < tileBehaviours.Length; i++) {
				TileBehaviour tileBehaviour = tileBehaviours[i];
				if (tileBehaviour == null) {
					continue;
				}

				if (!tileBehaviour.IsInNavGrid) {
					outOfBounds++;
				}

				if (tileBehaviour.transform.parent == null || tileBehaviour.transform.parent.GetComponent<TileFloor>() == null) {
					detachedFromTile++;
				}
			}

			if (outOfBounds > 0) {
				EditorGUILayout.HelpBox($"{outOfBounds} tile object(s) are outside NavGrid bounds.", MessageType.Warning);
			}

			if (detachedFromTile > 0) {
				EditorGUILayout.HelpBox($"{detachedFromTile} tile object(s) are not parented to a generated tile.", MessageType.Info);
			}

			if (level.PlayerStart != null && !level.PlayerStart.IsInNavGrid) {
				EditorGUILayout.HelpBox(level.PlayerStart.GridWarning, MessageType.Warning);
			}

			TileFloor[] tiles = level.GetTiles();
			if (tiles.Length != level.NavGrid.NodeCount) {
				EditorGUILayout.HelpBox($"Tile hierarchy is out of sync. Expected {level.NavGrid.NodeCount} tiles, found {tiles.Length}.", MessageType.Warning);
			}
		}

		private static void SyncLevel(LevelBehaviour level)
		{
			Undo.RecordObject(level.NavGrid, "Sync Level To Grid");
			level.PreviewNodesToGrid();
			EditorUtility.SetDirty(level);
			EditorUtility.SetDirty(level.NavGrid);
			SceneView.RepaintAll();
		}

		private static void RebuildTiles(LevelBehaviour level)
		{
			level.RebuildTiles();
			EditorUtility.SetDirty(level);
			if (level.NavGrid != null) {
				EditorUtility.SetDirty(level.NavGrid);
			}

			SceneView.RepaintAll();
		}

		private static void FloodFillFloor(LevelBehaviour level, Vector2Int startCell, GameObject replacementPrefab)
		{
			if (!level.TryGetTile(startCell, out TileFloor startTile) || startTile == null) {
				return;
			}

			GameObject sourcePrefab = GetPrefabSource(startTile.gameObject);
			if (sourcePrefab == replacementPrefab) {
				return;
			}

			FloodFill(
				level,
				startCell,
				cell => level.TryGetTile(cell, out TileFloor tile) && tile != null && GetPrefabSource(tile.gameObject) == sourcePrefab,
				cell => level.PaintTile(cell, replacementPrefab));
		}

		private static void FloodFillObjects(LevelBehaviour level, Vector2Int startCell, GameObject replacementPrefab)
		{
			GameObject sourcePrefab = GetTileBehaviourPrefab(level, startCell);
			if (sourcePrefab == replacementPrefab) {
				return;
			}

			FloodFill(
				level,
				startCell,
				cell => GetTileBehaviourPrefab(level, cell) == sourcePrefab,
				cell => level.PaintNode(cell, replacementPrefab));
		}

		private static void FloodFill(LevelBehaviour level, Vector2Int startCell, System.Func<Vector2Int, bool> matcher, System.Action<Vector2Int> apply)
		{
			if (!matcher(startCell)) {
				return;
			}

			Queue<Vector2Int> queue = new();
			HashSet<Vector2Int> visited = new();
			queue.Enqueue(startCell);
			visited.Add(startCell);

			while (queue.Count > 0) {
				Vector2Int cell = queue.Dequeue();
				apply(cell);

				for (int i = 0; i < 4; i++) {
					Vector2Int neighbor = GetNeighbor(cell, i);
					if (!level.NavGrid.IsInBounds(neighbor) || visited.Contains(neighbor) || !matcher(neighbor)) {
						continue;
					}

					visited.Add(neighbor);
					queue.Enqueue(neighbor);
				}
			}
		}

		private static Vector2Int GetNeighbor(Vector2Int cell, int index)
		{
			return index switch {
				0 => new Vector2Int(cell.x + 1, cell.y),
				1 => new Vector2Int(cell.x - 1, cell.y),
				2 => new Vector2Int(cell.x, cell.y + 1),
				_ => new Vector2Int(cell.x, cell.y - 1)
			};
		}

		private static GameObject GetTileBehaviourPrefab(LevelBehaviour level, Vector2Int cell)
		{
			return level.TryGetTileBehaviourAt(cell, out TileBehaviour tileBehaviour) && tileBehaviour != null
				? GetPrefabSource(tileBehaviour.gameObject)
				: null;
		}

		private static GameObject GetPrefabSource(GameObject instance)
		{
			return instance != null ? PrefabUtility.GetCorrespondingObjectFromSource(instance) : null;
		}

		private static bool TryGetHoveredCell(NavGrid navGrid, Vector2 guiPosition, out Vector2Int cell)
		{
			Ray   ray   = HandleUtility.GUIPointToWorldRay(guiPosition);
			Plane plane = new(navGrid.transform.up, navGrid.transform.position);
			if (!plane.Raycast(ray, out float distance)) {
				cell = default;
				return false;
			}

			Vector3 hitPoint = ray.GetPoint(distance);
			cell = navGrid.GetCellCoordinates(hitPoint);
			return navGrid.IsInBounds(cell);
		}

		private static void DrawHoveredCell(NavGrid navGrid, Vector2Int cell)
		{
			Vector3 origin   = navGrid.GetCellWorldCorner(cell.x, cell.y);
			Vector3 right    = navGrid.transform.right;
			Vector3 forward  = navGrid.transform.forward;
			Vector3[] corners = {
				origin,
				origin + forward,
				origin + forward + right,
				origin + right
			};

			Handles.DrawSolidRectangleWithOutline(corners, PaintPreviewColor, Color.clear);
		}
	}
}
