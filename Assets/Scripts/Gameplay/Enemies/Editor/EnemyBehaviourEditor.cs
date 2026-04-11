using System.Collections.Generic;
using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.BehaviourTree;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Enemies.Editor
{
	[CustomEditor(typeof(EnemyBehaviour), true)]
	public sealed class EnemyBehaviourEditor : UnityEditor.Editor
	{
		private void OnEnable()
		{
			EditorApplication.update += Repaint;
		}

		private void OnDisable()
		{
			EditorApplication.update -= Repaint;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EnemyBehaviour enemy = (EnemyBehaviour)target;
			if (!Application.isPlaying) {
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("Behaviour Tree debug becomes available in Play Mode.", MessageType.Info);
				return;
			}

			if (enemy.RuntimeHandle == null) {
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox(
					GetMissingRuntimeMessage(enemy),
					MessageType.Info
				);
				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Health", $"{enemy.RuntimeHandle.State.CurrentHealth}/{enemy.Config.MaxHealth}");
			EditorGUILayout.LabelField("Cell", enemy.RuntimeHandle.State.Position.ToString());
			EditorGUILayout.LabelField("Facing", enemy.RuntimeHandle.State.Facing.ToString());

			if (enemy.RuntimeHandle.PendingMortarCell.HasValue) {
				EditorGUILayout.LabelField("Pending Strike", enemy.RuntimeHandle.PendingMortarCell.Value.ToString());
				EditorGUILayout.LabelField("Turns Until Impact", enemy.RuntimeHandle.MortarTurnsUntilImpact.ToString());
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Behaviour Tree", EditorStyles.boldLabel);
			DrawTree(enemy.RuntimeHandle.DebugView);
		}

		private static void DrawTree(BehaviourTreeDebugView debugView)
		{
			IReadOnlyList<BehaviourTreeDebugLine> lines = debugView.Lines;
			if (lines.Count == 0) {
				EditorGUILayout.HelpBox("No behaviour tree snapshot recorded yet.", MessageType.None);
				return;
			}

			for (int i = 0; i < lines.Count; i++) {
				BehaviourTreeDebugLine line = lines[i];
				Rect row = EditorGUILayout.GetControlRect();
				row.xMin += line.Depth * 14.0f;

				Color previousColor = GUI.color;
				GUI.color = GetColor(line.Status);
				EditorGUI.LabelField(row, $"{line.Name} [{line.Status}]");
				GUI.color = previousColor;
			}
		}

		private static Color GetColor(BehaviourTreeNodeStatus status)
		{
			return status switch
			{
				BehaviourTreeNodeStatus.Success => new Color(0.45f, 0.8f, 0.45f),
				BehaviourTreeNodeStatus.Failure => new Color(0.95f, 0.45f, 0.45f),
				BehaviourTreeNodeStatus.Running => new Color(0.95f, 0.8f, 0.35f),
				_ => GUI.color
			};
		}

		private static string GetMissingRuntimeMessage(EnemyBehaviour enemy)
		{
			if (EditorUtility.IsPersistent(enemy) || PrefabUtility.IsPartOfPrefabAsset(enemy)) {
				return "This inspector is showing the prefab asset. Behaviour Tree debug is available only on the spawned runtime instance in the Hierarchy.";
			}

			if (PrefabStageUtility.GetCurrentPrefabStage() != null) {
				return "You are inspecting the prefab in Prefab Mode. Behaviour Tree debug is available only on the spawned runtime instance in the scene.";
			}

			return "This enemy does not have a runtime handle yet. Select the spawned enemy instance in the Hierarchy after it appears in the scene.";
		}
	}
}
