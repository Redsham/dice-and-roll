using Gameplay.Levels.Authoring;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Levels.Editor
{
	[CustomEditor(typeof(LevelBehaviour))]
	public sealed class LevelBehaviourEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUILayout.Space();

			if (GUILayout.Button("Preview Nodes To Grid")) {
				PreviewNodes((LevelBehaviour)target);
			}
		}

		private static void PreviewNodes(LevelBehaviour level)
		{
			Undo.RecordObject(level.NavGrid, "Preview Level Nodes");
			level.PreviewNodesToGrid();
			EditorUtility.SetDirty(level);
			EditorUtility.SetDirty(level.NavGrid);
			SceneView.RepaintAll();
		}
	}
}
