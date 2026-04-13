using System;
using System.Collections.Generic;
using Gameplay.Levels.Authoring;
using Gameplay.Nodes.Authoring;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Levels.Editor
{
	// === Palette Enums ===

	internal enum PaletteLayer
	{
		Floor  = 0,
		Object = 1
	}

	internal enum PaletteTool
	{
		Paint  = 0,
		Fill   = 1,
		Erase  = 2,
		Rotate = 3
	}

	internal enum PaletteObjectCategory
	{
		StaticObstacle       = 0,
		DestructibleObstacle = 1,
		CrushableProp        = 2,
		Other                = 3
	}

	// === Palette Settings ===

	[Serializable]
	internal sealed class ObjectPaletteEntry
	{
		public GameObject            Prefab;
		public PaletteObjectCategory Category;
	}

	[FilePath("UserSettings/LevelPaletteSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal sealed class LevelPaletteSettings : ScriptableSingleton<LevelPaletteSettings>
	{
		[SerializeField] private bool                    m_IsEnabled;
		[SerializeField] private PaletteLayer            m_ActiveLayer      = PaletteLayer.Floor;
		[SerializeField] private PaletteTool             m_ActiveTool       = PaletteTool.Paint;
		[SerializeField] private int                     m_SelectedFloor    = -1;
		[SerializeField] private int                     m_SelectedObject   = -1;
		[SerializeField] private List<GameObject>        m_FloorPrefabs     = new();
		[SerializeField] private List<ObjectPaletteEntry> m_ObjectPrefabs    = new();

		public bool IsEnabled
		{
			get => m_IsEnabled;
			set
			{
				m_IsEnabled = value;
				Save(true);
			}
		}

		public PaletteLayer ActiveLayer
		{
			get => m_ActiveLayer;
			set
			{
				m_ActiveLayer = value;
				Save(true);
			}
		}

		public PaletteTool ActiveTool
		{
			get => m_ActiveTool;
			set
			{
				m_ActiveTool = value;
				Save(true);
			}
		}

		public int SelectedFloor
		{
			get => m_SelectedFloor;
			set
			{
				m_SelectedFloor = value;
				Save(true);
			}
		}

		public int SelectedObject
		{
			get => m_SelectedObject;
			set
			{
				m_SelectedObject = value;
				Save(true);
			}
		}

		public List<GameObject> FloorPrefabs => m_FloorPrefabs;
		public List<ObjectPaletteEntry> ObjectPrefabs => m_ObjectPrefabs;

		public void SaveSettings()
		{
			Save(true);
		}
	}

	// === Palette State ===

	internal static class LevelPaletteState
	{
		public static LevelPaletteSettings Settings => LevelPaletteSettings.instance;

		public static bool IsEnabled
		{
			get => Settings.IsEnabled;
			set => Settings.IsEnabled = value;
		}

		public static PaletteLayer ActiveLayer
		{
			get => Settings.ActiveLayer;
			set => Settings.ActiveLayer = value;
		}

		public static GameObject SelectedFloorPrefab
		{
			get
			{
				int index = Settings.SelectedFloor;
				return index >= 0 && index < Settings.FloorPrefabs.Count ? Settings.FloorPrefabs[index] : null;
			}
		}

		public static PaletteTool ActiveTool
		{
			get => Settings.ActiveTool;
			set => Settings.ActiveTool = value;
		}

		public static ObjectPaletteEntry SelectedObjectEntry
		{
			get
			{
				int index = Settings.SelectedObject;
				return index >= 0 && index < Settings.ObjectPrefabs.Count ? Settings.ObjectPrefabs[index] : null;
			}
		}

		public static PaletteObjectCategory GuessCategory(GameObject prefab)
		{
			if (prefab == null) {
				return PaletteObjectCategory.Other;
			}

			TileBehaviour behaviour = prefab.GetComponent<TileBehaviour>();
			if (behaviour is StaticObstacleTileBehaviour) {
				return PaletteObjectCategory.StaticObstacle;
			}

			if (behaviour is DestructibleObstacleTileBehaviour) {
				return PaletteObjectCategory.DestructibleObstacle;
			}

			if (behaviour is CrushablePropTileBehaviour) {
				return PaletteObjectCategory.CrushableProp;
			}

			return PaletteObjectCategory.Other;
		}

		public static string GetCategoryLabel(PaletteObjectCategory category)
		{
			return category switch {
				PaletteObjectCategory.StaticObstacle       => "Static Obstacles",
				PaletteObjectCategory.DestructibleObstacle => "Destructible Obstacles",
				PaletteObjectCategory.CrushableProp        => "Crushable Props",
				_                                          => "Other"
			};
		}
	}
}
