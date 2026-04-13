using System.Collections.Generic;
using Gameplay.Levels.Authoring;
using Gameplay.Nodes.Authoring;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Levels.Editor
{
	public sealed class LevelPaletteWindow : EditorWindow
	{
		// === Constants ===

		private static readonly Vector2 FixedWindowSize = new(420f, 560f);
		private static GUIContent[] s_LayerContents;
		private static GUIContent[] s_ToolContents;
		private const float GridSpacing = 8f;
		private const float CardWidth = 92f;
		private const float CardHeight = 120f;
		private const float PreviewSize = 72f;

		// === State ===

		private Vector2 m_Scroll;

		[MenuItem("Tools/Level Palette")]
		public static void Open()
		{
			GetWindow<LevelPaletteWindow>("Level Palette");
		}

		private void OnEnable()
		{
			EnsureIconContent();
			minSize = FixedWindowSize;
			maxSize = FixedWindowSize;
		}

		private void OnGUI()
		{
			LevelPaletteSettings settings = LevelPaletteState.Settings;

			EditorGUILayout.LabelField("Palette", EditorStyles.boldLabel);
			LevelPaletteState.IsEnabled = EditorGUILayout.Toggle("Enabled", LevelPaletteState.IsEnabled);

			EditorGUILayout.Space(4f);
			DrawLayerButtons();

			EditorGUILayout.Space(6f);
			DrawToolToolbar();

			EditorGUILayout.Space();
			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
			switch (LevelPaletteState.ActiveLayer) {
				case PaletteLayer.Floor:
					DrawFloorLibrary(settings);
					break;
				case PaletteLayer.Object:
					DrawObjectLibrary(settings);
					break;
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Drag prefabs into the active layer library, select one, then use the Scene View tool: Paint, Fill, Erase or Rotate.", MessageType.Info);
		}

		private static void DrawLayerButtons()
		{
			EditorGUILayout.BeginHorizontal();
			DrawLayerButton(PaletteLayer.Floor);
			DrawLayerButton(PaletteLayer.Object);
			EditorGUILayout.EndHorizontal();
		}

		private static void DrawLayerButton(PaletteLayer layer)
		{
			bool isActive = LevelPaletteState.ActiveLayer == layer;
			GUIContent content = s_LayerContents[(int)layer];
			if (GUILayout.Toggle(isActive, content, "Button", GUILayout.Height(28f))) {
				LevelPaletteState.ActiveLayer = layer;
			}
		}

		private static void DrawToolToolbar()
		{
			EditorGUILayout.LabelField("Tool", EditorStyles.boldLabel);
			LevelPaletteState.ActiveTool = (PaletteTool)GUILayout.Toolbar((int)LevelPaletteState.ActiveTool, s_ToolContents, GUILayout.Height(28f));
		}

		private void DrawFloorLibrary(LevelPaletteSettings settings)
		{
			EditorGUILayout.LabelField("Floor Prefabs", EditorStyles.boldLabel);
			DrawAddFloorDropArea(settings);
			EditorGUILayout.Space(6f);
			DrawFloorGrid(settings);
			EditorGUILayout.Space(8f);
			DrawSelectedFloorDetails(settings);
		}

		private void DrawFloorGrid(LevelPaletteSettings settings)
		{
			if (settings.FloorPrefabs.Count == 0) {
				EditorGUILayout.HelpBox("Add at least one TileFloor prefab to paint the floor layer.", MessageType.Info);
				return;
			}

			DrawGrid(
				settings.FloorPrefabs.Count,
				index => DrawFloorCard(settings, index));
		}

		private void DrawFloorCard(LevelPaletteSettings settings, int index)
		{
			GameObject prefab = settings.FloorPrefabs[index];
			bool isSelected = settings.SelectedFloor == index;
			if (DrawPrefabCard(prefab, isSelected, true, out bool removeRequested)) {
				settings.SelectedFloor = index;
			}

			if (!removeRequested) {
				return;
			}

			settings.FloorPrefabs.RemoveAt(index);
			if (settings.SelectedFloor >= settings.FloorPrefabs.Count) {
				settings.SelectedFloor = settings.FloorPrefabs.Count - 1;
			}

			settings.SaveSettings();
			GUIUtility.ExitGUI();
		}

		private static void DrawSelectedFloorDetails(LevelPaletteSettings settings)
		{
			if (settings.SelectedFloor < 0 || settings.SelectedFloor >= settings.FloorPrefabs.Count) {
				return;
			}

			GameObject selectedPrefab = settings.FloorPrefabs[settings.SelectedFloor];
			EditorGUILayout.LabelField("Selected Floor", EditorStyles.boldLabel);
			GameObject nextPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", selectedPrefab, typeof(GameObject), false);
			if (nextPrefab != selectedPrefab) {
				settings.FloorPrefabs[settings.SelectedFloor] = nextPrefab;
				settings.SaveSettings();
			}

			if (nextPrefab != null && nextPrefab.GetComponent<TileFloor>() == null) {
				EditorGUILayout.HelpBox("Floor prefab must contain a TileFloor component.", MessageType.Warning);
			}
		}

		private void DrawObjectLibrary(LevelPaletteSettings settings)
		{
			EditorGUILayout.LabelField("Object Prefabs", EditorStyles.boldLabel);
			DrawAddObjectDropArea(settings);
			EditorGUILayout.Space(6f);
			DrawObjectCategoryGrid(settings, PaletteObjectCategory.StaticObstacle);
			DrawObjectCategoryGrid(settings, PaletteObjectCategory.DestructibleObstacle);
			DrawObjectCategoryGrid(settings, PaletteObjectCategory.CrushableProp);
			DrawObjectCategoryGrid(settings, PaletteObjectCategory.Other);
			EditorGUILayout.Space(8f);
			DrawSelectedObjectDetails(settings);
		}

		private void DrawObjectCategoryGrid(LevelPaletteSettings settings, PaletteObjectCategory category)
		{
			List<int> indices = new();
			for (int i = 0; i < settings.ObjectPrefabs.Count; i++) {
				if (settings.ObjectPrefabs[i].Category == category) {
					indices.Add(i);
				}
			}

			if (indices.Count == 0) {
				return;
			}

			EditorGUILayout.LabelField(LevelPaletteState.GetCategoryLabel(category), EditorStyles.miniBoldLabel);
			DrawGrid(
				indices.Count,
				localIndex => DrawObjectCard(settings, indices[localIndex]));
			EditorGUILayout.Space(6f);
		}

		private void DrawObjectCard(LevelPaletteSettings settings, int index)
		{
			ObjectPaletteEntry entry = settings.ObjectPrefabs[index];
			bool isSelected = settings.SelectedObject == index;
			if (DrawPrefabCard(entry.Prefab, isSelected, true, out bool removeRequested)) {
				settings.SelectedObject = index;
			}

			if (!removeRequested) {
				return;
			}

			settings.ObjectPrefabs.RemoveAt(index);
			if (settings.SelectedObject >= settings.ObjectPrefabs.Count) {
				settings.SelectedObject = settings.ObjectPrefabs.Count - 1;
			}

			settings.SaveSettings();
			GUIUtility.ExitGUI();
		}

		private static void DrawSelectedObjectDetails(LevelPaletteSettings settings)
		{
			if (settings.SelectedObject < 0 || settings.SelectedObject >= settings.ObjectPrefabs.Count) {
				return;
			}

			ObjectPaletteEntry entry = settings.ObjectPrefabs[settings.SelectedObject];
			EditorGUILayout.LabelField("Selected Object", EditorStyles.boldLabel);

			GameObject nextPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", entry.Prefab, typeof(GameObject), false);
			if (nextPrefab != entry.Prefab) {
				entry.Prefab = nextPrefab;
				entry.Category = LevelPaletteState.GuessCategory(nextPrefab);
				settings.SaveSettings();
			}

			PaletteObjectCategory nextCategory = (PaletteObjectCategory)EditorGUILayout.EnumPopup("Category", entry.Category);
			if (nextCategory != entry.Category) {
				entry.Category = nextCategory;
				settings.SaveSettings();
			}

			if (entry.Prefab != null && entry.Prefab.GetComponent<TileBehaviour>() == null) {
				EditorGUILayout.HelpBox("Object prefab must contain a TileBehaviour component.", MessageType.Warning);
			}
		}

		private void DrawGrid(int itemCount, System.Action<int> drawItem)
		{
			float availableWidth = position.width - 24f;
			int columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + GridSpacing) / (CardWidth + GridSpacing)));
			int rows = Mathf.CeilToInt(itemCount / (float)columns);

			for (int row = 0; row < rows; row++) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				for (int column = 0; column < columns; column++) {
					int index = row * columns + column;
					if (index >= itemCount) {
						break;
					}

					drawItem(index);
					if (column < columns - 1) {
						GUILayout.Space(GridSpacing);
					}
				}

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(GridSpacing);
			}
		}

		private static bool DrawPrefabCard(GameObject prefab, bool isSelected, bool allowRemove, out bool removeRequested)
		{
			Rect cardRect = GUILayoutUtility.GetRect(CardWidth, CardHeight, GUILayout.Width(CardWidth), GUILayout.Height(CardHeight));
			removeRequested = false;

			GUI.Box(cardRect, GUIContent.none, EditorStyles.helpBox);
			if (isSelected) {
				EditorGUI.DrawRect(cardRect, new Color(0.24f, 0.54f, 0.88f, 0.18f));
				Handles.BeginGUI();
				Color previousColor = Handles.color;
				Handles.color = new Color(0.24f, 0.54f, 0.88f, 1f);
				Handles.DrawAAPolyLine(2f,
					new Vector3(cardRect.xMin, cardRect.yMin),
					new Vector3(cardRect.xMax, cardRect.yMin),
					new Vector3(cardRect.xMax, cardRect.yMax),
					new Vector3(cardRect.xMin, cardRect.yMax),
					new Vector3(cardRect.xMin, cardRect.yMin));
				Handles.color = previousColor;
				Handles.EndGUI();
			}

			Rect previewRect = new(
				cardRect.x + (cardRect.width - PreviewSize) * 0.5f,
				cardRect.y + 8f,
				PreviewSize,
				PreviewSize);
			EditorGUI.DrawRect(previewRect, new Color(0.16f, 0.16f, 0.16f, 1f));

			Texture previewTexture = GetPreviewTexture(prefab);
			if (previewTexture != null) {
				GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit, true);
			}

			Rect labelRect = new(cardRect.x + 6f, cardRect.yMax - 34f, cardRect.width - 12f, 28f);
			EditorGUI.LabelField(labelRect, prefab != null ? prefab.name : "Missing", GetCardLabelStyle());

			if (allowRemove) {
				Rect removeRect = new(cardRect.xMax - 22f, cardRect.y + 4f, 18f, 18f);
				if (GUI.Button(removeRect, "x")) {
					removeRequested = true;
				}
			}

			return GUI.Button(cardRect, GUIContent.none, GUIStyle.none);
		}

		private static Texture GetPreviewTexture(GameObject prefab)
		{
			if (prefab == null) {
				return null;
			}

			Texture preview = AssetPreview.GetAssetPreview(prefab);
			return preview != null ? preview : AssetPreview.GetMiniThumbnail(prefab);
		}

		private static GUIStyle GetCardLabelStyle()
		{
			GUIStyle style = new(EditorStyles.miniLabel) {
				alignment = TextAnchor.UpperCenter,
				wordWrap = true
			};
			return style;
		}

		private static GUIContent CreateIconContent(string iconName, string tooltip)
		{
			GUIContent content = EditorGUIUtility.IconContent(iconName);
			if (content == null) {
				return new GUIContent(string.Empty, tooltip);
			}

			content.tooltip = tooltip;
			return content;
		}

		private static void EnsureIconContent()
		{
			if (s_LayerContents == null) {
				s_LayerContents = new[] {
					CreateIconContent("Terrain Icon", "Floor Layer"),
					CreateIconContent("Prefab Icon", "Object Layer")
				};
			}

			if (s_ToolContents == null) {
				s_ToolContents = new[] {
					CreateIconContent("Grid.PaintTool", "Paint"),
					CreateIconContent("Grid.BoxTool", "Flood Fill"),
					CreateIconContent("Grid.EraserTool", "Erase"),
					CreateIconContent("RotateTool", "Rotate")
				};
			}
		}

		private static void DrawAddFloorDropArea(LevelPaletteSettings settings)
		{
			Rect dropRect = EditorGUILayout.GetControlRect(false, 40f);
			GUI.Box(dropRect, "Drag TileFloor prefab here");
			HandleDrop(dropRect, prefab => {
				if (prefab.GetComponent<TileFloor>() == null) {
					return;
				}

				settings.FloorPrefabs.Add(prefab);
				settings.SelectedFloor = settings.FloorPrefabs.Count - 1;
				settings.SaveSettings();
			});
		}

		private static void DrawAddObjectDropArea(LevelPaletteSettings settings)
		{
			Rect dropRect = EditorGUILayout.GetControlRect(false, 40f);
			GUI.Box(dropRect, "Drag TileBehaviour prefab here");
			HandleDrop(dropRect, prefab => {
				if (prefab.GetComponent<TileBehaviour>() == null) {
					return;
				}

				settings.ObjectPrefabs.Add(new ObjectPaletteEntry {
					Prefab = prefab,
					Category = LevelPaletteState.GuessCategory(prefab)
				});
				settings.SelectedObject = settings.ObjectPrefabs.Count - 1;
				settings.SaveSettings();
			});
		}

		private static void HandleDrop(Rect dropRect, System.Action<GameObject> onAcceptedPrefab)
		{
			Event currentEvent = Event.current;
			if (!dropRect.Contains(currentEvent.mousePosition)) {
				return;
			}

			if (currentEvent.type == EventType.DragUpdated) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				currentEvent.Use();
				return;
			}

			if (currentEvent.type != EventType.DragPerform) {
				return;
			}

			DragAndDrop.AcceptDrag();
			for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
				if (DragAndDrop.objectReferences[i] is GameObject prefab) {
					onAcceptedPrefab(prefab);
				}
			}

			currentEvent.Use();
		}
	}
}
