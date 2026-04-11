using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace EditorTools.TextureTools.Editor
{
	public sealed class TextureResizeWindow : EditorWindow
	{
		private const string DefaultProfilePath = "Assets/Scripts/EditorTools/TextureTools/Editor/TextureResizeProfile.asset";

		private TextureResizeProfile m_Profile;
		private Vector2              m_SourcesScroll;
		private Vector2              m_PreviewScroll;
		private string               m_StatusMessage = "Ready";

		[MenuItem("Tools/Texture Tools/Texture Resizer")]
		public static void Open()
		{
			TextureResizeWindow window = GetWindow<TextureResizeWindow>();
			window.titleContent = new("Texture Resizer");
			window.minSize      = new(1100f, 720f);
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
				DrawConfigurationPanel();
				DrawPreviewPanel();
			}

			using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
				EditorGUILayout.LabelField(m_StatusMessage, EditorStyles.miniLabel);
			}
		}

		private void DrawToolbar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
				TextureResizeProfile newProfile = (TextureResizeProfile)EditorGUILayout.ObjectField(m_Profile, typeof(TextureResizeProfile), false, GUILayout.Width(320f));
				if (newProfile != null && newProfile != m_Profile)
					m_Profile = newProfile;

				if (GUILayout.Button("Create Profile", EditorStyles.toolbarButton, GUILayout.Width(95f)))
					CreateProfileAsset();

				if (GUILayout.Button("Use Selection", EditorStyles.toolbarButton, GUILayout.Width(95f)))
					ApplySelection();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Run Resize", EditorStyles.toolbarButton, GUILayout.Width(95f)))
					RunResize();
			}
		}

		private void DrawConfigurationPanel()
		{
			using (new EditorGUILayout.VerticalScope(GUILayout.Width(420f))) {
				EditorGUILayout.LabelField("Resize Configuration", EditorStyles.boldLabel);
				m_Profile.OperationMode       = (TextureResizeOperationMode)EditorGUILayout.EnumPopup("Operation",     m_Profile.OperationMode);
				m_Profile.ScalingMode         = (TextureResizeScalingMode)EditorGUILayout.EnumPopup("Scaling",         m_Profile.ScalingMode);
				m_Profile.PowerOfTwoMode      = (TextureResizePowerOfTwoMode)EditorGUILayout.EnumPopup("Power Of Two", m_Profile.PowerOfTwoMode);
				m_Profile.IncludeSubfolders   = EditorGUILayout.Toggle("Include Subfolders",    m_Profile.IncludeSubfolders);
				m_Profile.OverwriteExisting   = EditorGUILayout.Toggle("Overwrite Existing",    m_Profile.OverwriteExisting);
				m_Profile.PreserveTextureType = EditorGUILayout.Toggle("Preserve Texture Type", m_Profile.PreserveTextureType);

				switch (m_Profile.ScalingMode) {
					case TextureResizeScalingMode.Exact:
					case TextureResizeScalingMode.FitWithin:
						m_Profile.Width  = EditorGUILayout.IntPopup("Width",  Mathf.Max(1, m_Profile.Width),  SizeLabelStrings(), SizeValues());
						m_Profile.Height = EditorGUILayout.IntPopup("Height", Mathf.Max(1, m_Profile.Height), SizeLabelStrings(), SizeValues());
						break;
					case TextureResizeScalingMode.LongSide:
						m_Profile.LongSide = EditorGUILayout.IntPopup("Long Side", Mathf.Max(1, m_Profile.LongSide), SizeLabelStrings(), SizeValues());
						break;
					case TextureResizeScalingMode.ShortSide:
						m_Profile.ShortSide = EditorGUILayout.IntPopup("Short Side", Mathf.Max(1, m_Profile.ShortSide), SizeLabelStrings(), SizeValues());
						break;
					case TextureResizeScalingMode.Percent:
						m_Profile.Percent = EditorGUILayout.IntSlider("Percent", Mathf.Clamp(m_Profile.Percent, 1, 400), 1, 400);
						break;
				}

				if (m_Profile.OperationMode == TextureResizeOperationMode.BakeCopies)
					m_Profile.OutputDirectory = EditorGUILayout.TextField("Output Folder", m_Profile.OutputDirectory);

				if (m_Profile.OperationMode == TextureResizeOperationMode.ImportMaxSize) {
					EditorGUILayout.HelpBox(
					                        "Import Max Size is non-destructive and fast. Unity constrains the imported resolution by the longest side, so Exact/Fit modes are approximated to the nearest supported max size.",
					                        MessageType.Info);
				}
				else {
					EditorGUILayout.HelpBox(
					                        "Bake Copies creates new PNG textures at the resolved size. Originals are not modified.",
					                        MessageType.Info);
				}

				EditorGUILayout.Space(8f);
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField($"Sources ({m_Profile.Sources.Count})", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Clear", GUILayout.Width(70f))) {
						Undo.RecordObject(m_Profile, "Clear Resize Sources");
						m_Profile.Sources.Clear();
						EditorUtility.SetDirty(m_Profile);
					}
				}

				m_SourcesScroll = EditorGUILayout.BeginScrollView(m_SourcesScroll, GUI.skin.box);
				for (int i = 0; i < m_Profile.Sources.Count; i++) {
					using (new EditorGUILayout.HorizontalScope()) {
						m_Profile.Sources[i] = EditorGUILayout.ObjectField(m_Profile.Sources[i], typeof(Object), false);
						if (GUILayout.Button("X", GUILayout.Width(26f))) {
							Undo.RecordObject(m_Profile, "Remove Resize Source");
							m_Profile.Sources.RemoveAt(i);
							EditorUtility.SetDirty(m_Profile);
							GUIUtility.ExitGUI();
						}
					}
				}
				EditorGUILayout.EndScrollView();

				Rect dropRect = GUILayoutUtility.GetRect(0f, 54f, GUILayout.ExpandWidth(true));
				GUI.Box(dropRect, "Drop textures or folders here");
				HandleDropArea(dropRect);

				if (GUI.changed)
					EditorUtility.SetDirty(m_Profile);
			}
		}

		private void DrawPreviewPanel()
		{
			List<Texture2D> textures = TextureToolsEditorUtility.CollectTextures(m_Profile.Sources, m_Profile.IncludeSubfolders);

			using (new EditorGUILayout.VerticalScope()) {
				EditorGUILayout.LabelField($"Resolved Textures ({textures.Count})", EditorStyles.boldLabel);
				m_PreviewScroll = EditorGUILayout.BeginScrollView(m_PreviewScroll, GUI.skin.box);

				if (textures.Count == 0) {
					EditorGUILayout.HelpBox("Add textures or folders to preview the resulting resolutions.", MessageType.Info);
				}
				else {
					for (int i = 0; i < textures.Count; i++)
						DrawPreviewRow(textures[i]);
				}

				EditorGUILayout.EndScrollView();
			}
		}

		private void DrawPreviewRow(Texture2D texture)
		{
			Vector2Int targetSize = TextureToolsEditorUtility.CalculateTargetSize(texture.width, texture.height, m_Profile);
			using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
				GUILayout.Label(AssetPreview.GetAssetPreview(texture) ?? AssetPreview.GetMiniThumbnail(texture), GUILayout.Width(54f), GUILayout.Height(54f));

				using (new EditorGUILayout.VerticalScope()) {
					EditorGUILayout.LabelField(texture.name,                                                           EditorStyles.boldLabel);
					EditorGUILayout.LabelField(TextureToolsEditorUtility.GetAssetPath(texture),                        EditorStyles.miniLabel);
					EditorGUILayout.LabelField($"{texture.width}x{texture.height}  ->  {targetSize.x}x{targetSize.y}", EditorStyles.label);
				}

				GUILayout.FlexibleSpace();
				if (m_Profile.OperationMode == TextureResizeOperationMode.ImportMaxSize) {
					int maxSize = TextureToolsEditorUtility.ClosestSupportedMaxSize(Mathf.Max(targetSize.x, targetSize.y));
					EditorGUILayout.LabelField($"Importer max: {maxSize}", GUILayout.Width(120f));
				}
				else {
					EditorGUILayout.LabelField("Bake copy", GUILayout.Width(120f));
				}
			}
		}

		private void ApplySelection()
		{
			Undo.RecordObject(m_Profile, "Set Resize Sources From Selection");
			m_Profile.Sources.Clear();
			m_Profile.Sources.AddRange(Selection.objects);
			EditorUtility.SetDirty(m_Profile);
			m_StatusMessage = $"Loaded {m_Profile.Sources.Count} object(s) from the current selection.";
		}

		private void RunResize()
		{
			List<Texture2D> textures = TextureToolsEditorUtility.CollectTextures(m_Profile.Sources, m_Profile.IncludeSubfolders);
			if (textures.Count == 0) {
				m_StatusMessage = "No textures to process.";
				return;
			}

			try {
				switch (m_Profile.OperationMode) {
					case TextureResizeOperationMode.ImportMaxSize:
						ApplyImportResize(textures);
						break;
					case TextureResizeOperationMode.BakeCopies:
						BakeResizedCopies(textures);
						break;
				}
			} catch (Exception exception) {
				m_StatusMessage = $"Resize failed: {exception.Message}";
				Debug.LogException(exception);
			}
		}

		private void ApplyImportResize(IReadOnlyList<Texture2D> textures)
		{
			int updated = 0;
			for (int i = 0; i < textures.Count; i++) {
				Texture2D       texture  = textures[i];
				string          path     = AssetDatabase.GetAssetPath(texture);
				TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
				if (importer == null)
					continue;

				Vector2Int targetSize     = TextureToolsEditorUtility.CalculateTargetSize(texture.width, texture.height, m_Profile);
				int        desiredMaxSize = TextureToolsEditorUtility.ClosestSupportedMaxSize(Mathf.Max(targetSize.x, targetSize.y));
				if (importer.maxTextureSize == desiredMaxSize)
					continue;

				importer.maxTextureSize = desiredMaxSize;
				if (!m_Profile.PreserveTextureType)
					importer.textureType = TextureImporterType.Default;
				importer.SaveAndReimport();
				updated++;
			}

			m_StatusMessage = $"Updated import max size for {updated} texture(s).";
		}

		private void BakeResizedCopies(IReadOnlyList<Texture2D> textures)
		{
			TextureToolsEditorUtility.EnsureFolderExists(m_Profile.OutputDirectory);
			int written = 0;

			for (int i = 0; i < textures.Count; i++) {
				Texture2D  texture    = textures[i];
				Vector2Int targetSize = TextureToolsEditorUtility.CalculateTargetSize(texture.width, texture.height, m_Profile);
				Texture2D  resized    = TextureToolsEditorUtility.ResizeTexture(texture, targetSize.x, targetSize.y, FilterMode.Bilinear);
				if (resized == null)
					continue;

				string fileName   = $"{TextureToolsEditorUtility.SanitizeFileName(texture.name)}_{targetSize.x}x{targetSize.y}.png";
				string outputPath = TextureToolsEditorUtility.CombineAssetPath(m_Profile.OutputDirectory, fileName);
				if (File.Exists(outputPath) && !m_Profile.OverwriteExisting) {
					DestroyImmediate(resized);
					continue;
				}

				File.WriteAllBytes(outputPath, resized.EncodeToPNG());
				DestroyImmediate(resized);
				AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);

				TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
				if (importer != null && m_Profile.PreserveTextureType) {
					TextureImporter sourceImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
					if (sourceImporter != null) {
						importer.textureType         = sourceImporter.textureType;
						importer.alphaIsTransparency = sourceImporter.alphaIsTransparency;
						importer.wrapMode            = sourceImporter.wrapMode;
						importer.filterMode          = sourceImporter.filterMode;
						importer.mipmapEnabled       = sourceImporter.mipmapEnabled;
						importer.spriteImportMode    = sourceImporter.spriteImportMode;
						importer.SaveAndReimport();
					}
				}

				written++;
			}

			AssetDatabase.Refresh();
			m_StatusMessage = $"Baked {written} resized texture copy/copies to {m_Profile.OutputDirectory}.";
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
					Undo.RecordObject(m_Profile, "Add Resize Sources");
					m_Profile.Sources.AddRange(DragAndDrop.objectReferences);
					EditorUtility.SetDirty(m_Profile);
				}

				current.Use();
			}
		}

		private static string[] SizeLabelStrings()
		{
			return new[] {
				"64", "128",
				"256", "512",
				"1024", "2048",
				"4096", "8192"
			};
		}

		private static int[] SizeValues()
		{
			return new[] {
				64, 128,
				256, 512,
				1024, 2048,
				4096, 8192
			};
		}

		private TextureResizeProfile LoadOrCreateDefaultProfile()
		{
			TextureResizeProfile profile = AssetDatabase.LoadAssetAtPath<TextureResizeProfile>(DefaultProfilePath);
			if (profile != null)
				return profile;

			TextureToolsEditorUtility.EnsureFolderExists("Assets/Scripts/EditorTools/TextureTools/Editor");
			profile = CreateInstance<TextureResizeProfile>();
			AssetDatabase.CreateAsset(profile, DefaultProfilePath);
			AssetDatabase.SaveAssets();
			return profile;
		}

		private void CreateProfileAsset()
		{
			string path = EditorUtility.SaveFilePanelInProject(
			                                                   "Create Resize Profile",
			                                                   "TextureResizeProfile",
			                                                   "asset",
			                                                   "Choose where to store the texture resize profile.");

			if (string.IsNullOrEmpty(path))
				return;

			TextureResizeProfile profile = CreateInstance<TextureResizeProfile>();
			AssetDatabase.CreateAsset(profile, path);
			AssetDatabase.SaveAssets();
			m_Profile       = profile;
			m_StatusMessage = $"Created profile at {path}";
		}
	}
}