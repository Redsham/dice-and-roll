using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Levels.Editor
{
	[CustomEditor(typeof(TileFloor))]
	public sealed class TileFloorEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("TileFloor placement is tool-driven. Move, paint and replace floor tiles through the level palette.", MessageType.Info);
			DrawDefaultInspector();
		}

		private void OnSceneGUI()
		{
			TileFloor tile = (TileFloor)target;
			if (tile == null) {
				return;
			}

			LevelBehaviour level = tile.GetComponentInParent<LevelBehaviour>();
			if (level == null || level.NavGrid == null) {
				return;
			}

			NavGrid navGrid = level.NavGrid;
			Vector2Int cell = tile.GridPosition;
			Vector3 alignedPosition = tile.GetAlignedWorldPosition(navGrid, cell);

			if (tile.transform.position == alignedPosition) {
				return;
			}

			Undo.RecordObject(tile.transform, "Restore TileFloor Position");
			tile.transform.position = alignedPosition;
			EditorUtility.SetDirty(tile.transform);
			SceneView.RepaintAll();
		}
	}
}
