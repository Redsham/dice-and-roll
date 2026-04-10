using UnityEditor;
using UnityEngine;


namespace Gameplay.Navigation.Editor
{
	[CustomEditor(typeof(NavGrid))]
	public class NavGridEditor : UnityEditor.Editor
	{
		// === Constants ===

		private static readonly Color GridColor = new(1f, 1f, 1f, 0.35f);
		private static readonly Color BlockedFillColor = new(1f, 0.15f, 0.15f, 0.35f);
		private static readonly Color BlockedOutlineColor = new(1f, 0.25f, 0.25f, 0.85f);

		// === State ===

		private readonly Vector3[] m_CellCorners = new Vector3[4];

		// === Inspector ===

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUILayout.Space();

			if (GUILayout.Button("Rebuild Grid")) {
				RebuildGrid((NavGrid)target);
			}
		}

		// === Scene ===

		private void OnSceneGUI()
		{
			NavGrid navGrid = (NavGrid)target;
			if (navGrid == null) {
				return;
			}

			DrawGrid(navGrid);
			DrawBlockedCells(navGrid);
		}

		private void DrawGrid(NavGrid navGrid)
		{
			Handles.color = GridColor;

			for (int y = 0; y < navGrid.Height; y++) {
				for (int x = 0; x < navGrid.Width; x++) {
					GetCellCorners(navGrid, x, y, m_CellCorners);
					Handles.DrawPolyLine(m_CellCorners[0], m_CellCorners[1], m_CellCorners[2], m_CellCorners[3], m_CellCorners[0]);
				}
			}
		}

		private void DrawBlockedCells(NavGrid navGrid)
		{
			if (navGrid.Nodes.Data == null || navGrid.Nodes.Data.Length != navGrid.NodeCount) {
				return;
			}

			for (int y = 0; y < navGrid.Height; y++) {
				for (int x = 0; x < navGrid.Width; x++) {
					if (navGrid.Nodes[x, y].IsWalkable) {
						continue;
					}

					GetCellCorners(navGrid, x, y, m_CellCorners);
					Handles.DrawSolidRectangleWithOutline(m_CellCorners, BlockedFillColor, BlockedOutlineColor);
				}
			}
		}

		// === Helpers ===

		private static void GetCellCorners(NavGrid navGrid, int x, int y, Vector3[] corners)
		{
			Vector3 origin = navGrid.GetCellWorldCorner(x, y);
			Vector3 right = navGrid.transform.right;
			Vector3 forward = navGrid.transform.forward;

			corners[0] = origin;
			corners[1] = origin + forward;
			corners[2] = origin + forward + right;
			corners[3] = origin + right;
		}

		private static void RebuildGrid(NavGrid navGrid)
		{
			Undo.RecordObject(navGrid, "Rebuild Navigation Grid");
			navGrid.RebuildGrid();
			EditorUtility.SetDirty(navGrid);
			SceneView.RepaintAll();
		}
	}
}
