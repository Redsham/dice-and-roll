using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;


namespace EditorTools.TextureTools.Editor
{
	public sealed class TextureAtlasComposerWindow : EditorWindow
	{
		private const string DEFAULT_PROFILE_PATH = "Assets/Scripts/EditorTools/TextureTools/Editor/TextureAtlasComposerProfile.asset";
		private const float  LEFT_PANEL_WIDTH     = 340f;
		private const float  PREVIEW_PADDING      = 16f;

		private TextureAtlasComposerProfile m_Profile;
		private Vector2                     m_EntriesScroll;
		private Vector2                     m_InspectorScroll;
		private Vector2                     m_PreviewPan;
		private float                       m_PreviewZoom   = 1f;
		private int                         m_SelectedIndex = -1;
		private bool                        m_IsDraggingEntry;
		private Vector2                     m_DragStartMousePosition;
		private RectInt                     m_DragStartRect;
		private string                      m_StatusMessage = "Ready";

		[MenuItem("Tools/Texture Tools/Atlas Composer")]
		public static void Open()
		{
			TextureAtlasComposerWindow window = GetWindow<TextureAtlasComposerWindow>();
			window.titleContent = new("Atlas Composer");
			window.minSize      = new(1280f, 760f);
			window.Show();
		}

		private void OnEnable()
		{
			m_Profile = LoadOrCreateDefaultProfile();
		}

		private void OnGUI()
		{
			if (m_Profile == null)
				m_Profile = LoadOrCreateDefaultProfile();

			DrawToolbar();

			using (new EditorGUILayout.HorizontalScope()) {
				DrawEntriesPanel();
				DrawPreviewAndInspector();
			}

			DrawStatusBar();
		}

		private void DrawToolbar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
				TextureAtlasComposerProfile newProfile = (TextureAtlasComposerProfile)EditorGUILayout.ObjectField(
				                                                                                                  m_Profile,
				                                                                                                  typeof(TextureAtlasComposerProfile),
				                                                                                                  false,
				                                                                                                  GUILayout.Width(320f));

				if (newProfile != m_Profile && newProfile != null) {
					m_Profile       = newProfile;
					m_SelectedIndex = Mathf.Clamp(m_SelectedIndex, -1, m_Profile.Entries.Count - 1);
				}

				if (GUILayout.Button("Create Profile", EditorStyles.toolbarButton, GUILayout.Width(95f)))
					CreateProfileAsset();

				if (GUILayout.Button("Add Selection", EditorStyles.toolbarButton, GUILayout.Width(95f)))
					AddSelection();

				if (GUILayout.Button("Auto Layout", EditorStyles.toolbarButton, GUILayout.Width(90f)))
					AutoLayout();

				if (GUILayout.Button("Normalize Size", EditorStyles.toolbarButton, GUILayout.Width(105f)))
					NormalizeSelectedSize();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Generate Atlas", EditorStyles.toolbarButton, GUILayout.Width(110f)))
					GenerateAtlas();
			}
		}

		private void DrawEntriesPanel()
		{
			using (new EditorGUILayout.VerticalScope(GUILayout.Width(LEFT_PANEL_WIDTH))) {
				EditorGUILayout.LabelField("Atlas Profile", EditorStyles.boldLabel);
				using (new EditorGUI.ChangeCheckScope()) {
					m_Profile.OutputDirectory     = EditorGUILayout.TextField("Output Folder", m_Profile.OutputDirectory);
					m_Profile.OutputName          = EditorGUILayout.TextField("Output Name",   m_Profile.OutputName);
					m_Profile.AtlasWidth          = EditorGUILayout.IntPopup("Atlas Width",  Mathf.Max(32, m_Profile.AtlasWidth),  SizeLabelStrings(), SizeValues());
					m_Profile.AtlasHeight         = EditorGUILayout.IntPopup("Atlas Height", Mathf.Max(32, m_Profile.AtlasHeight), SizeLabelStrings(), SizeValues());
					m_Profile.AutoLayoutPadding   = EditorGUILayout.IntSlider("Auto Padding", Mathf.Max(0, m_Profile.AutoLayoutPadding), 0, 64);
					m_Profile.ClearColor          = EditorGUILayout.ColorField("Clear Color", m_Profile.ClearColor);
					m_Profile.FilterMode          = (FilterMode)EditorGUILayout.EnumPopup("Filter",    m_Profile.FilterMode);
					m_Profile.WrapMode            = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap", m_Profile.WrapMode);
					m_Profile.AutoCreateSprites   = EditorGUILayout.ToggleLeft("Create multiple sprites on import", m_Profile.AutoCreateSprites);
					m_Profile.GenerateLayoutAsset = EditorGUILayout.ToggleLeft("Generate layout asset",             m_Profile.GenerateLayoutAsset);
					m_Profile.OverwriteExisting   = EditorGUILayout.ToggleLeft("Overwrite existing PNG",            m_Profile.OverwriteExisting);

					if (GUI.changed)
						EditorUtility.SetDirty(m_Profile);
				}

				EditorGUILayout.Space(8f);
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField($"Entries ({m_Profile.Entries.Count})", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Clear", GUILayout.Width(70f))) {
						if (EditorUtility.DisplayDialog("Clear entries", "Remove all atlas entries from the current profile?", "Clear", "Cancel")) {
							Undo.RecordObject(m_Profile, "Clear Atlas Entries");
							m_Profile.Entries.Clear();
							m_SelectedIndex = -1;
							EditorUtility.SetDirty(m_Profile);
						}
					}
				}

				m_EntriesScroll = EditorGUILayout.BeginScrollView(m_EntriesScroll, GUI.skin.box);
				for (int i = 0; i < m_Profile.Entries.Count; i++)
					DrawEntryRow(i, m_Profile.Entries[i]);
				EditorGUILayout.EndScrollView();

				Rect dropRect = GUILayoutUtility.GetRect(0f, 54f, GUILayout.ExpandWidth(true));
				GUI.Box(dropRect, "Drop textures or folders here");
				HandleDropArea(dropRect);
			}
		}

		private void DrawEntryRow(int index, TextureAtlasSourceEntry entry)
		{
			Rect rowRect    = EditorGUILayout.BeginVertical(GUI.skin.box);
			bool isSelected = index == m_SelectedIndex;

			if (isSelected)
				EditorGUI.DrawRect(rowRect, new(0.2f, 0.45f, 0.8f, 0.18f));

			EditorGUI.BeginChangeCheck();
			using (new EditorGUILayout.HorizontalScope()) {
				entry.Enabled = EditorGUILayout.Toggle(entry.Enabled, GUILayout.Width(18f));

				Texture2D preview = entry.Texture;
				GUILayout.Label(AssetPreview.GetAssetPreview(preview) ?? AssetPreview.GetMiniThumbnail(preview), GUILayout.Width(28f), GUILayout.Height(28f));

				using (new EditorGUILayout.VerticalScope()) {
					EditorGUILayout.LabelField(entry.DisplayName,                                                                                         EditorStyles.boldLabel);
					EditorGUILayout.LabelField($"{entry.Destination.width}x{entry.Destination.height} at ({entry.Destination.x}, {entry.Destination.y})", EditorStyles.miniLabel);
				}

				GUILayout.FlexibleSpace();
				entry.SortOrder = EditorGUILayout.IntField(entry.SortOrder, GUILayout.Width(38f));
			}

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Select", GUILayout.Width(56f)))
					m_SelectedIndex = index;

				if (GUILayout.Button("Fit", GUILayout.Width(44f))) {
					Undo.RecordObject(m_Profile, "Fit Atlas Entry");
					FitEntryToTexture(entry);
					EditorUtility.SetDirty(m_Profile);
				}

				GUI.enabled = index > 0;
				if (GUILayout.Button("Up", GUILayout.Width(40f))) {
					Undo.RecordObject(m_Profile, "Move Atlas Entry");
					(m_Profile.Entries[index - 1], m_Profile.Entries[index]) = (m_Profile.Entries[index], m_Profile.Entries[index - 1]);
					m_SelectedIndex                                          = index - 1;
					EditorUtility.SetDirty(m_Profile);
				}

				GUI.enabled = index < m_Profile.Entries.Count - 1;
				if (GUILayout.Button("Down", GUILayout.Width(52f))) {
					Undo.RecordObject(m_Profile, "Move Atlas Entry");
					(m_Profile.Entries[index + 1], m_Profile.Entries[index]) = (m_Profile.Entries[index], m_Profile.Entries[index + 1]);
					m_SelectedIndex                                          = index + 1;
					EditorUtility.SetDirty(m_Profile);
				}

				GUI.enabled = true;
				if (GUILayout.Button("Remove", GUILayout.Width(70f))) {
					Undo.RecordObject(m_Profile, "Remove Atlas Entry");
					m_Profile.Entries.RemoveAt(index);
					m_SelectedIndex = Mathf.Clamp(m_SelectedIndex, -1, m_Profile.Entries.Count - 1);
					EditorUtility.SetDirty(m_Profile);
					EditorGUILayout.EndVertical();
					return;
				}
			}

			if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition)) {
				m_SelectedIndex = index;
				Repaint();
			}

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(m_Profile);

			EditorGUILayout.EndVertical();
		}

		private void DrawPreviewAndInspector()
		{
			using (new EditorGUILayout.VerticalScope()) {
				Rect previewRect = GUILayoutUtility.GetRect(10f, 10_000f, 320f, 10_000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				DrawPreview(previewRect);

				EditorGUILayout.Space(6f);
				m_InspectorScroll = EditorGUILayout.BeginScrollView(m_InspectorScroll, GUI.skin.box, GUILayout.Height(260f));
				DrawSelectedEntryInspector();
				EditorGUILayout.EndScrollView();
			}
		}

		private void DrawPreview(Rect previewRect)
		{
			GUI.Box(previewRect, GUIContent.none);
			Rect innerRect = new(previewRect.x + PREVIEW_PADDING, previewRect.y + PREVIEW_PADDING, previewRect.width - PREVIEW_PADDING * 2f, previewRect.height - PREVIEW_PADDING * 2f);
			EditorGUI.DrawRect(innerRect, new(0.13f, 0.13f, 0.13f));

			if (m_Profile.AtlasWidth <= 0 || m_Profile.AtlasHeight <= 0)
				return;

			float   baseScale       = Mathf.Min(innerRect.width  / m_Profile.AtlasWidth, innerRect.height / m_Profile.AtlasHeight);
			float   scale           = Mathf.Max(0.05f, baseScale * m_PreviewZoom);
			Vector2 atlasScreenSize = new(m_Profile.AtlasWidth   * scale, m_Profile.AtlasHeight * scale);
			Rect atlasRect = new(
			                     innerRect.center.x - atlasScreenSize.x * 0.5f + m_PreviewPan.x,
			                     innerRect.center.y - atlasScreenSize.y * 0.5f + m_PreviewPan.y,
			                     atlasScreenSize.x,
			                     atlasScreenSize.y);

			EditorGUI.DrawRect(atlasRect, m_Profile.ClearColor.gamma);
			DrawCheckerBackground(atlasRect, 16f);
			GUI.BeginGroup(atlasRect);
			Matrix4x4 previousMatrix = GUI.matrix;

			List<TextureAtlasSourceEntry> orderedEntries = m_Profile.Entries.OrderBy(entry => entry.SortOrder).ToList();
			for (int i = 0; i < orderedEntries.Count; i++) {
				TextureAtlasSourceEntry entry = orderedEntries[i];
				if (!entry.Enabled || entry.Texture == null)
					continue;

				Rect    screenRect = AtlasToPreviewRect(entry.Destination, scale);
				Vector2 pivot      = new(screenRect.x + screenRect.width * entry.Pivot.x, screenRect.y + screenRect.height * entry.Pivot.y);
				GUIUtility.RotateAroundPivot(entry.RotationDegrees, pivot);

				Color previousColor = GUI.color;
				GUI.color = entry.Tint;
				GUI.DrawTexture(screenRect, entry.Texture, ScaleMode.StretchToFill, true);
				GUI.color  = previousColor;
				GUI.matrix = previousMatrix;

				Color outlineColor = ReferenceEquals(entry, SelectedEntry()) ? new(0.27f, 0.74f, 1f) : new Color(1f, 1f, 1f, 0.35f);
				Handles.BeginGUI();
				Handles.color = outlineColor;
				Handles.DrawAAPolyLine(2f,
				                       new Vector3(screenRect.xMin, screenRect.yMin),
				                       new Vector3(screenRect.xMax, screenRect.yMin),
				                       new Vector3(screenRect.xMax, screenRect.yMax),
				                       new Vector3(screenRect.xMin, screenRect.yMax),
				                       new Vector3(screenRect.xMin, screenRect.yMin));
				Handles.EndGUI();
			}

			GUI.matrix = previousMatrix;
			GUI.EndGroup();

			DrawPreviewOverlay(atlasRect, scale);
			HandlePreviewInput(previewRect, atlasRect, scale);
		}

		private void DrawSelectedEntryInspector()
		{
			TextureAtlasSourceEntry entry = SelectedEntry();
			if (entry == null) {
				EditorGUILayout.HelpBox("Select an atlas entry to edit its placement, size and export settings.", MessageType.Info);
				return;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField(entry.DisplayName, EditorStyles.boldLabel);
			entry.Texture         = (Texture2D)EditorGUILayout.ObjectField("Texture", entry.Texture, typeof(Texture2D), false);
			entry.Id              = EditorGUILayout.TextField("Identifier", entry.Id);
			entry.Enabled         = EditorGUILayout.Toggle("Enabled", entry.Enabled);
			entry.SortOrder       = EditorGUILayout.IntField("Sort Order", entry.SortOrder);
			entry.PreserveAspect  = EditorGUILayout.Toggle("Preserve Aspect", entry.PreserveAspect);
			entry.Tint            = EditorGUILayout.ColorField("Tint", entry.Tint);
			entry.Pivot           = EditorGUILayout.Vector2Field("Pivot", entry.Pivot);
			entry.Pivot           = new(Mathf.Clamp01(entry.Pivot.x), Mathf.Clamp01(entry.Pivot.y));
			entry.RotationDegrees = EditorGUILayout.Slider("Rotation", entry.RotationDegrees, -180f, 180f);

			RectInt destination = entry.Destination;
			EditorGUILayout.Space(6f);
			EditorGUILayout.LabelField("Destination Rect", EditorStyles.miniBoldLabel);

			EditorGUI.BeginChangeCheck();
			int x      = EditorGUILayout.IntField("X", destination.x);
			int y      = EditorGUILayout.IntField("Y", destination.y);
			int width  = Mathf.Max(1, EditorGUILayout.IntField("Width",  destination.width));
			int height = Mathf.Max(1, EditorGUILayout.IntField("Height", destination.height));
			if (EditorGUI.EndChangeCheck()) {
				if (entry.PreserveAspect && entry.Texture != null && (width != destination.width || height != destination.height)) {
					float aspect = entry.Texture.width / (float)Mathf.Max(1, entry.Texture.height);
					if (width != destination.width)
						height = Mathf.Max(1, Mathf.RoundToInt(width / aspect));
					else
						width = Mathf.Max(1, Mathf.RoundToInt(height * aspect));
				}

				entry.Destination = new(x, y, width, height);
			}

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Fit To Source")) {
					Undo.RecordObject(m_Profile, "Fit Atlas Entry");
					FitEntryToTexture(entry);
				}

				if (GUILayout.Button("Center In Atlas")) {
					Undo.RecordObject(m_Profile, "Center Atlas Entry");
					entry.Destination = new(
					                        (m_Profile.AtlasWidth  - entry.Destination.width)  / 2,
					                        (m_Profile.AtlasHeight - entry.Destination.height) / 2,
					                        entry.Destination.width,
					                        entry.Destination.height);
				}

				if (GUILayout.Button("Snap Inside")) {
					Undo.RecordObject(m_Profile, "Clamp Atlas Entry");
					entry.Destination = ClampToAtlas(entry.Destination);
				}
			}

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Nudge Left")) {
					Undo.RecordObject(m_Profile, "Nudge Atlas Entry");
					entry.Destination = MoveRect(entry.Destination, -1, 0);
				}
				if (GUILayout.Button("Nudge Right")) {
					Undo.RecordObject(m_Profile, "Nudge Atlas Entry");
					entry.Destination = MoveRect(entry.Destination, 1, 0);
				}
				if (GUILayout.Button("Nudge Up")) {
					Undo.RecordObject(m_Profile, "Nudge Atlas Entry");
					entry.Destination = MoveRect(entry.Destination, 0, -1);
				}
				if (GUILayout.Button("Nudge Down")) {
					Undo.RecordObject(m_Profile, "Nudge Atlas Entry");
					entry.Destination = MoveRect(entry.Destination, 0, 1);
				}
			}

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(m_Profile);
		}

		private void DrawPreviewOverlay(Rect atlasRect, float scale)
		{
			Handles.BeginGUI();
			Handles.color = new(1f, 1f, 1f, 0.28f);
			Handles.DrawSolidRectangleWithOutline(atlasRect, Color.clear, new(1f, 1f, 1f, 0.45f));

			for (int x = 0; x <= m_Profile.AtlasWidth; x += 128) {
				float previewX = atlasRect.x + x * scale;
				Handles.DrawLine(new(previewX, atlasRect.y), new(previewX, atlasRect.yMax));
			}

			for (int y = 0; y <= m_Profile.AtlasHeight; y += 128) {
				float previewY = atlasRect.y + y * scale;
				Handles.DrawLine(new(atlasRect.x, previewY), new(atlasRect.xMax, previewY));
			}

			Handles.EndGUI();
			GUI.Label(new(atlasRect.x + 8f, atlasRect.y + 8f, 220f, 22f), $"{m_Profile.AtlasWidth} x {m_Profile.AtlasHeight}");
		}

		private void HandlePreviewInput(Rect previewRect, Rect atlasRect, float scale)
		{
			Event current = Event.current;
			if (!previewRect.Contains(current.mousePosition))
				return;

			if (current.type == EventType.ScrollWheel) {
				float zoomDelta = -current.delta.y * 0.05f;
				m_PreviewZoom = Mathf.Clamp(m_PreviewZoom + zoomDelta, 0.2f, 8f);
				current.Use();
			}

			if (current.type == EventType.MouseDown && current.button == 0) {
				int hitIndex = FindEntryAtPosition(current.mousePosition, atlasRect, scale);
				if (hitIndex >= 0) {
					Undo.RecordObject(m_Profile, "Move Atlas Entry");
					m_SelectedIndex          = hitIndex;
					m_IsDraggingEntry        = true;
					m_DragStartMousePosition = current.mousePosition;
					m_DragStartRect          = SelectedEntry().Destination;
					current.Use();
				}
			}

			if (current.type == EventType.MouseDown && current.button == 2) {
				m_DragStartMousePosition = current.mousePosition;
				current.Use();
			}

			if (current.type == EventType.MouseDrag && current.button == 2) {
				m_PreviewPan             += current.mousePosition - m_DragStartMousePosition;
				m_DragStartMousePosition =  current.mousePosition;
				current.Use();
				Repaint();
			}

			if (m_IsDraggingEntry && current.type == EventType.MouseDrag && current.button == 0) {
				Vector2                 delta = (current.mousePosition - m_DragStartMousePosition) / Mathf.Max(0.0001f, scale);
				TextureAtlasSourceEntry entry = SelectedEntry();
				entry.Destination = ClampToAtlas(new(
				                                     m_DragStartRect.x + Mathf.RoundToInt(delta.x),
				                                     m_DragStartRect.y + Mathf.RoundToInt(delta.y),
				                                     m_DragStartRect.width,
				                                     m_DragStartRect.height));
				EditorUtility.SetDirty(m_Profile);
				current.Use();
				Repaint();
			}

			if (current.type == EventType.MouseUp)
				m_IsDraggingEntry = false;
		}

		private void HandleDropArea(Rect dropRect)
		{
			Event current = Event.current;
			if (!dropRect.Contains(current.mousePosition))
				return;

			if (current.type is EventType.DragUpdated or EventType.DragPerform) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				if (current.type == EventType.DragPerform) {
					DragAndDrop.AcceptDrag();
					List<Texture2D> textures = TextureToolsEditorUtility.CollectTextures(DragAndDrop.objectReferences, true);
					AddTextures(textures);
				}

				current.Use();
			}
		}

		private void AddSelection()
		{
			List<Texture2D> textures = TextureToolsEditorUtility.CollectTextures(Selection.objects, true);
			AddTextures(textures);
		}

		private void AddTextures(IReadOnlyList<Texture2D> textures)
		{
			if (textures == null || textures.Count == 0) {
				m_StatusMessage = "No textures were found in the provided selection.";
				return;
			}

			Undo.RecordObject(m_Profile, "Add Atlas Textures");
			HashSet<string> existingPaths = new(m_Profile.Entries
			                                             .Where(entry => entry.Texture != null)
			                                             .Select(entry => TextureToolsEditorUtility.GetAssetPath(entry.Texture)));

			int addedCount = 0;
			for (int i = 0; i < textures.Count; i++) {
				Texture2D texture = textures[i];
				string    path    = TextureToolsEditorUtility.GetAssetPath(texture);
				if (string.IsNullOrEmpty(path) || !existingPaths.Add(path))
					continue;

				TextureAtlasSourceEntry entry = new() {
					Texture        = texture,
					Id             = texture.name,
					Destination    = new(0, 0, texture.width, texture.height),
					PreserveAspect = true,
					Tint           = Color.white
				};

				m_Profile.Entries.Add(entry);
				addedCount++;
			}

			if (addedCount > 0) {
				EditorUtility.SetDirty(m_Profile);
				AutoLayout();
				m_SelectedIndex = Mathf.Max(0, m_Profile.Entries.Count - 1);
			}

			m_StatusMessage = addedCount > 0
				                  ? $"Added {addedCount} texture(s) to the atlas profile."
				                  : "All dropped textures are already present in the atlas profile.";
		}

		private void AutoLayout()
		{
			Undo.RecordObject(m_Profile, "Auto Layout Atlas");
			int  cursorX   = m_Profile.AutoLayoutPadding;
			int  cursorY   = m_Profile.AutoLayoutPadding;
			int  rowHeight = 0;
			int  placed    = 0;
			bool overflow  = false;

			for (int i = 0; i < m_Profile.Entries.Count; i++) {
				TextureAtlasSourceEntry entry = m_Profile.Entries[i];
				if (!entry.Enabled || entry.Texture == null)
					continue;

				FitEntryToTexture(entry);

				int width  = entry.Destination.width;
				int height = entry.Destination.height;
				if (cursorX + width + m_Profile.AutoLayoutPadding > m_Profile.AtlasWidth) {
					cursorX   =  m_Profile.AutoLayoutPadding;
					cursorY   += rowHeight + m_Profile.AutoLayoutPadding;
					rowHeight =  0;
				}

				if (cursorY + height + m_Profile.AutoLayoutPadding > m_Profile.AtlasHeight) {
					overflow = true;
					break;
				}

				entry.Destination =  new(cursorX, cursorY, width, height);
				entry.SortOrder   =  placed;
				cursorX           += width + m_Profile.AutoLayoutPadding;
				rowHeight         =  Mathf.Max(rowHeight, height);
				placed++;
			}

			EditorUtility.SetDirty(m_Profile);
			m_StatusMessage = overflow
				                  ? "Auto layout finished, but some textures do not fit into the selected atlas size."
				                  : $"Auto layout placed {placed} texture(s).";
		}

		private void NormalizeSelectedSize()
		{
			TextureAtlasSourceEntry entry = SelectedEntry();
			if (entry?.Texture == null)
				return;

			Undo.RecordObject(m_Profile, "Normalize Atlas Entry Size");
			FitEntryToTexture(entry);
			EditorUtility.SetDirty(m_Profile);
		}

		private void FitEntryToTexture(TextureAtlasSourceEntry entry)
		{
			if (entry.Texture == null)
				return;

			RectInt rect = entry.Destination;
			rect.width        = entry.Texture.width;
			rect.height       = entry.Texture.height;
			entry.Destination = ClampToAtlas(rect);
		}

		private void GenerateAtlas()
		{
			try {
				TextureToolsEditorUtility.EnsureFolderExists(m_Profile.OutputDirectory);
				string outputName = TextureToolsEditorUtility.SanitizeFileName(m_Profile.OutputName);
				string pngPath    = TextureToolsEditorUtility.CombineAssetPath(m_Profile.OutputDirectory, $"{outputName}.png");
				if (File.Exists(pngPath) && !m_Profile.OverwriteExisting) {
					m_StatusMessage = $"Skipped export because {pngPath} already exists and overwrite is disabled.";
					return;
				}

				List<TextureAtlasSpriteEntry> exportedEntries = new();
				Texture2D                     atlasTexture    = BuildAtlasTexture(exportedEntries);
				byte[]                        pngBytes        = atlasTexture.EncodeToPNG();
				File.WriteAllBytes(pngPath, pngBytes);
				DestroyImmediate(atlasTexture);

				AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
				Texture2D atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
				ConfigureAtlasImporter(pngPath, exportedEntries);

				if (m_Profile.GenerateLayoutAsset)
					WriteLayoutAsset(outputName, atlasAsset, exportedEntries);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				m_StatusMessage = $"Atlas exported to {pngPath}";
			} catch (Exception exception) {
				m_StatusMessage = $"Atlas export failed: {exception.Message}";
				Debug.LogException(exception);
			}
		}

		private Texture2D BuildAtlasTexture(ICollection<TextureAtlasSpriteEntry> exportedEntries)
		{
			Texture2D atlas = new(m_Profile.AtlasWidth, m_Profile.AtlasHeight, TextureFormat.RGBA32, false, false) {
				filterMode = m_Profile.FilterMode,
				wrapMode   = m_Profile.WrapMode
			};

			Color[] pixels = Enumerable.Repeat(m_Profile.ClearColor, m_Profile.AtlasWidth * m_Profile.AtlasHeight).ToArray();

			foreach (TextureAtlasSourceEntry entry in m_Profile.Entries.OrderBy(item => item.SortOrder)) {
				if (!entry.Enabled || entry.Texture == null || entry.Destination.width <= 0 || entry.Destination.height <= 0)
					continue;

				RectInt   rect     = ClampToAtlas(entry.Destination);
				Texture2D readable = TextureToolsEditorUtility.CreateReadableCopy(entry.Texture);
				if (readable == null)
					continue;

				CompositeEntry(pixels, readable, rect, entry);
				exportedEntries.Add(new() {
					Id              = entry.DisplayName,
					SourceAssetPath = TextureToolsEditorUtility.GetAssetPath(entry.Texture),
					PixelRect       = new(rect.x, m_Profile.AtlasHeight - rect.y - rect.height, rect.width, rect.height),
					Pivot           = entry.Pivot,
					RotationDegrees = entry.RotationDegrees,
					UvRect = new(
					             rect.x                                         / (float)m_Profile.AtlasWidth,
					             (m_Profile.AtlasHeight - rect.y - rect.height) / (float)m_Profile.AtlasHeight,
					             rect.width                                     / (float)m_Profile.AtlasWidth,
					             rect.height                                    / (float)m_Profile.AtlasHeight)
				});

				DestroyImmediate(readable);
			}

			atlas.SetPixels(pixels);
			atlas.Apply(false, false);
			return atlas;
		}

		private void CompositeEntry(Color[] atlasPixels, Texture2D source, RectInt destination, TextureAtlasSourceEntry entry)
		{
			float   radians     = entry.RotationDegrees * Mathf.Deg2Rad;
			float   cosine      = Mathf.Cos(-radians);
			float   sine        = Mathf.Sin(-radians);
			Vector2 pivotPixels = new(destination.width * entry.Pivot.x, destination.height * entry.Pivot.y);

			for (int y = 0; y < destination.height; y++) {
				for (int x = 0; x < destination.width; x++) {
					Vector2 centered = new Vector2(x + 0.5f, y + 0.5f) - pivotPixels;
					Vector2 unrotated = new Vector2(
					                                centered.x * cosine - centered.y * sine,
					                                centered.x * sine   + centered.y * cosine) + pivotPixels;

					float u = unrotated.x / Mathf.Max(1f, destination.width);
					float v = unrotated.y / Mathf.Max(1f, destination.height);
					if (u < 0f || u > 1f || v < 0f || v > 1f)
						continue;

					Color sampled = source.GetPixelBilinear(u, v) * entry.Tint;
					if (sampled.a <= 0f)
						continue;

					int atlasX = destination.x                                              + x;
					int atlasY = m_Profile.AtlasHeight - destination.y - destination.height + y;
					if (atlasX < 0 || atlasX >= m_Profile.AtlasWidth || atlasY < 0 || atlasY >= m_Profile.AtlasHeight)
						continue;

					int pixelIndex = atlasY * m_Profile.AtlasWidth + atlasX;
					atlasPixels[pixelIndex] = AlphaBlend(atlasPixels[pixelIndex], sampled);
				}
			}
		}

		private void ConfigureAtlasImporter(string pngPath, IEnumerable<TextureAtlasSpriteEntry> entries)
		{
			TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
			if (importer == null)
				return;

			importer.textureType         = TextureImporterType.Sprite;
			importer.spriteImportMode    = m_Profile.AutoCreateSprites ? SpriteImportMode.Multiple : SpriteImportMode.Single;
			importer.alphaIsTransparency = true;
			importer.mipmapEnabled       = false;
			importer.isReadable          = false;
			importer.filterMode          = m_Profile.FilterMode;
			importer.wrapMode            = m_Profile.WrapMode;

			if (m_Profile.AutoCreateSprites) {
				SpriteDataProviderFactories dataProviderFactories = new();
				dataProviderFactories.Init();
				ISpriteEditorDataProvider dataProvider = dataProviderFactories.GetSpriteEditorDataProviderFromObject(importer);
				if (dataProvider == null) {
					importer.SaveAndReimport();
					return;
				}

				dataProvider.InitSpriteEditorDataProvider();
				List<SpriteRect> spriteRects = new();
				foreach (TextureAtlasSpriteEntry entry in entries) {
					spriteRects.Add(new() {
						name      = entry.Id,
						rect      = entry.PixelRect,
						pivot     = entry.Pivot,
						alignment = SpriteAlignment.Custom,
						spriteID  = GUID.Generate()
					});
				}

				dataProvider.SetSpriteRects(spriteRects.ToArray());
				dataProvider.Apply();
			}

			importer.SaveAndReimport();
		}

		private void WriteLayoutAsset(string outputName, Texture2D atlasTexture, IReadOnlyCollection<TextureAtlasSpriteEntry> entries)
		{
			string                  assetPath   = TextureToolsEditorUtility.CombineAssetPath(m_Profile.OutputDirectory, $"{outputName}_Layout.asset");
			TextureAtlasLayoutAsset layoutAsset = AssetDatabase.LoadAssetAtPath<TextureAtlasLayoutAsset>(assetPath);
			if (layoutAsset == null) {
				layoutAsset = CreateInstance<TextureAtlasLayoutAsset>();
				AssetDatabase.CreateAsset(layoutAsset, assetPath);
			}

			layoutAsset.SetData(atlasTexture, new(m_Profile.AtlasWidth, m_Profile.AtlasHeight), entries.ToList());
			EditorUtility.SetDirty(layoutAsset);
		}

		private TextureAtlasSourceEntry SelectedEntry()
		{
			if (m_SelectedIndex < 0 || m_SelectedIndex >= m_Profile.Entries.Count)
				return null;

			return m_Profile.Entries[m_SelectedIndex];
		}

		private int FindEntryAtPosition(Vector2 mousePosition, Rect atlasRect, float scale)
		{
			for (int i = m_Profile.Entries.Count - 1; i >= 0; i--) {
				TextureAtlasSourceEntry entry = m_Profile.Entries[i];
				Rect rect = new(
				                atlasRect.x + entry.Destination.x * scale,
				                atlasRect.y + entry.Destination.y * scale,
				                entry.Destination.width  * scale,
				                entry.Destination.height * scale);

				if (rect.Contains(mousePosition))
					return i;
			}

			return -1;
		}

		private Rect AtlasToPreviewRect(RectInt rect, float scale)
		{
			return new(rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale);
		}

		private RectInt ClampToAtlas(RectInt rect)
		{
			rect.width  = Mathf.Clamp(rect.width,  1, m_Profile.AtlasWidth);
			rect.height = Mathf.Clamp(rect.height, 1, m_Profile.AtlasHeight);
			rect.x      = Mathf.Clamp(rect.x,      0, Mathf.Max(0, m_Profile.AtlasWidth  - rect.width));
			rect.y      = Mathf.Clamp(rect.y,      0, Mathf.Max(0, m_Profile.AtlasHeight - rect.height));
			return rect;
		}

		private RectInt MoveRect(RectInt rect, int deltaX, int deltaY)
		{
			return ClampToAtlas(new(rect.x + deltaX, rect.y + deltaY, rect.width, rect.height));
		}

		private static Color AlphaBlend(Color background, Color foreground)
		{
			float outAlpha = foreground.a + background.a * (1f - foreground.a);
			if (outAlpha <= 0f)
				return Color.clear;

			float outRed   = (foreground.r * foreground.a + background.r * background.a * (1f - foreground.a)) / outAlpha;
			float outGreen = (foreground.g * foreground.a + background.g * background.a * (1f - foreground.a)) / outAlpha;
			float outBlue  = (foreground.b * foreground.a + background.b * background.a * (1f - foreground.a)) / outAlpha;
			return new(outRed, outGreen, outBlue, outAlpha);
		}

		private static void DrawCheckerBackground(Rect rect, float cellSize)
		{
			Color first   = new(0.18f, 0.18f, 0.18f);
			Color second  = new(0.24f, 0.24f, 0.24f);
			int   rows    = Mathf.CeilToInt(rect.height / cellSize);
			int   columns = Mathf.CeilToInt(rect.width  / cellSize);
			for (int row = 0; row < rows; row++) {
				for (int column = 0; column < columns; column++) {
					Color color = (row + column) % 2 == 0 ? first : second;
					EditorGUI.DrawRect(new(rect.x + column * cellSize, rect.y + row * cellSize, cellSize, cellSize), color);
				}
			}
		}

		private void DrawStatusBar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
				EditorGUILayout.LabelField(m_StatusMessage, EditorStyles.miniLabel);
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("LMB: select/drag  MMB: pan  Wheel: zoom", EditorStyles.miniLabel, GUILayout.Width(220f));
			}
		}

		private static string[] SizeLabelStrings()
		{
			return new[] {
				"256", "512",
				"1024", "2048",
				"4096", "8192"
			};
		}

		private static int[] SizeValues()
		{
			return new[] {
				256, 512,
				1024, 2048,
				4096, 8192
			};
		}

		private TextureAtlasComposerProfile LoadOrCreateDefaultProfile()
		{
			TextureAtlasComposerProfile profile = AssetDatabase.LoadAssetAtPath<TextureAtlasComposerProfile>(DEFAULT_PROFILE_PATH);
			if (profile != null)
				return profile;

			TextureToolsEditorUtility.EnsureFolderExists("Assets/Scripts/EditorTools/TextureTools/Editor");
			profile = CreateInstance<TextureAtlasComposerProfile>();
			AssetDatabase.CreateAsset(profile, DEFAULT_PROFILE_PATH);
			AssetDatabase.SaveAssets();
			return profile;
		}

		private void CreateProfileAsset()
		{
			string path = EditorUtility.SaveFilePanelInProject(
			                                                   "Create Atlas Profile",
			                                                   "TextureAtlasComposerProfile",
			                                                   "asset",
			                                                   "Choose where to store the atlas composer profile.");

			if (string.IsNullOrEmpty(path))
				return;

			TextureAtlasComposerProfile profile = CreateInstance<TextureAtlasComposerProfile>();
			AssetDatabase.CreateAsset(profile, path);
			AssetDatabase.SaveAssets();
			m_Profile       = profile;
			m_StatusMessage = $"Created profile at {path}";
		}
	}
}