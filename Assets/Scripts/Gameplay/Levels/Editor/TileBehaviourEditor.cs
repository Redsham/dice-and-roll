using Gameplay.Levels.Authoring;
using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Levels.Editor
{
	[CustomEditor(typeof(TileBehaviour), true)]
	public sealed class TileBehaviourEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			TileBehaviour node = (TileBehaviour)target;

			if (!node.IsInNavGrid && !string.IsNullOrWhiteSpace(node.GridWarning)) {
				EditorGUILayout.HelpBox(node.GridWarning, MessageType.Warning);
			}

			if (node.transform.parent == null || node.transform.parent.GetComponent<TileFloor>() == null) {
				EditorGUILayout.HelpBox("TileBehaviour is not attached to a generated TileFloor in the hierarchy.", MessageType.Info);
			}

			EditorGUILayout.HelpBox("Hold Shift while moving a TileBehaviour to snap it to the grid using the selected pivot mode.", MessageType.None);

			DrawDefaultInspector();
		}

		private void OnSceneGUI()
		{
			TileBehaviour node = (TileBehaviour)target;
			Event         currentEvent = Event.current;
			if (node == null || currentEvent == null || !currentEvent.shift) {
				return;
			}

			if (currentEvent.type != EventType.MouseDrag && currentEvent.type != EventType.MouseUp) {
				return;
			}

			LevelBehaviour level = node.GetComponentInParent<LevelBehaviour>();
			if (level == null || level.NavGrid == null) {
				return;
			}

			SnapNodeToGrid(node, level);
		}

		private static void SnapNodeToGrid(TileBehaviour node, LevelBehaviour level)
		{
			NavGrid    navGrid       = level.NavGrid;
			Vector2Int targetCell    = navGrid.GetCellCoordinates(node.transform.position, node.Pivot);
			Transform  nodeTransform = node.transform;

			if (level.TryGetTile(targetCell, out TileFloor tile) && tile != null && nodeTransform.parent != tile.transform) {
				Undo.SetTransformParent(nodeTransform, tile.transform, "Attach Node To Tile");
			}

			node.SetGridPosition(targetCell);
			EditorUtility.SetDirty(node);
			EditorUtility.SetDirty(nodeTransform);
			SceneView.RepaintAll();
		}
	}
}
