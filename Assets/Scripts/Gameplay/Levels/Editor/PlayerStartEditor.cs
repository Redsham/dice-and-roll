using Gameplay.Levels.Authoring;
using UnityEditor;


namespace Gameplay.Levels.Editor
{
	[CustomEditor(typeof(PlayerStart))]
	public sealed class PlayerStartEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			PlayerStart playerStart = (PlayerStart)target;
			if (!playerStart.IsInNavGrid && !string.IsNullOrWhiteSpace(playerStart.GridWarning)) {
				EditorGUILayout.HelpBox(playerStart.GridWarning, MessageType.Warning);
			}

			DrawDefaultInspector();
		}
	}
}
