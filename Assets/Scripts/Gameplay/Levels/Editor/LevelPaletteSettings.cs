using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Levels.Editor
{
	internal sealed class LevelPaletteSettings : ScriptableObject
	{
		internal const string AssetPath = "Assets/Settings/Editor/LevelPaletteSettings.asset";

		private static LevelPaletteSettings s_Instance;

		[SerializeField] private bool                     m_IsEnabled;
		[SerializeField] private PaletteLayer             m_ActiveLayer    = PaletteLayer.Floor;
		[SerializeField] private PaletteTool              m_ActiveTool     = PaletteTool.Paint;
		[SerializeField] private int                      m_SelectedFloor  = -1;
		[SerializeField] private int                      m_SelectedObject = -1;
		[SerializeField] private List<GameObject>         m_FloorPrefabs   = new();
		[SerializeField] private List<ObjectPaletteEntry> m_ObjectPrefabs  = new();

		public static LevelPaletteSettings Instance
		{
			get
			{
				if (s_Instance != null) {
					return s_Instance;
				}

				s_Instance = LoadOrCreate();
				return s_Instance;
			}
		}

		public bool IsEnabled
		{
			get => m_IsEnabled;
			set
			{
				if (m_IsEnabled == value) {
					return;
				}

				m_IsEnabled = value;
				SaveSettings();
			}
		}

		public PaletteLayer ActiveLayer
		{
			get => m_ActiveLayer;
			set
			{
				if (m_ActiveLayer == value) {
					return;
				}

				m_ActiveLayer = value;
				SaveSettings();
			}
		}

		public PaletteTool ActiveTool
		{
			get => m_ActiveTool;
			set
			{
				if (m_ActiveTool == value) {
					return;
				}

				m_ActiveTool = value;
				SaveSettings();
			}
		}

		public int SelectedFloor
		{
			get => m_SelectedFloor;
			set
			{
				if (m_SelectedFloor == value) {
					return;
				}

				m_SelectedFloor = value;
				SaveSettings();
			}
		}

		public int SelectedObject
		{
			get => m_SelectedObject;
			set
			{
				if (m_SelectedObject == value) {
					return;
				}

				m_SelectedObject = value;
				SaveSettings();
			}
		}

		public List<GameObject> FloorPrefabs => m_FloorPrefabs;
		public List<ObjectPaletteEntry> ObjectPrefabs => m_ObjectPrefabs;

		public void SaveSettings()
		{
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		private static LevelPaletteSettings LoadOrCreate()
		{
			LevelPaletteSettings settings = AssetDatabase.LoadAssetAtPath<LevelPaletteSettings>(AssetPath);
			if (settings != null) {
				return settings;
			}

			if (File.Exists(AssetPath)) {
				AssetDatabase.DeleteAsset(AssetPath);
			}

			string directoryPath = Path.GetDirectoryName(AssetPath);
			if (!string.IsNullOrEmpty(directoryPath) && !AssetDatabase.IsValidFolder(directoryPath)) {
				EnsureFolderExists(directoryPath);
			}

			settings = CreateInstance<LevelPaletteSettings>();
			AssetDatabase.CreateAsset(settings, AssetPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return settings;
		}

		private static void EnsureFolderExists(string folderPath)
		{
			string normalizedPath = folderPath.Replace('\\', '/');
			string[] parts = normalizedPath.Split('/');
			string currentPath = parts[0];

			for (int i = 1; i < parts.Length; i++) {
				string nextPath = $"{currentPath}/{parts[i]}";
				if (!AssetDatabase.IsValidFolder(nextPath)) {
					AssetDatabase.CreateFolder(currentPath, parts[i]);
				}

				currentPath = nextPath;
			}
		}
	}
}
