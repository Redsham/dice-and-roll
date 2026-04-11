using UnityEditor;
using UnityEngine;


namespace Gameplay.Navigation.Editor
{
	[CustomEditor(typeof(NavGrid))]
	public sealed class NavGridEditor : UnityEditor.Editor
	{
		private static readonly Color GridColor                = new(1f, 1f, 1f, 0.35f);
		private static readonly Color StaticPropFillColor      = new(0.72f, 0.24f, 0.18f, 0.45f);
		private static readonly Color StaticPropOutlineColor   = new(0.85f, 0.34f, 0.28f, 0.95f);
		private static readonly Color DestructibleFillColor    = new(0.95f, 0.56f, 0.16f, 0.38f);
		private static readonly Color DestructibleOutlineColor = new(1.0f, 0.72f, 0.28f, 0.95f);
		private static readonly Color DecorativeFillColor      = new(0.82f, 0.78f, 0.18f, 0.2f);
		private static readonly Color DecorativeOutlineColor   = new(0.98f, 0.92f, 0.35f, 0.9f);
		private static readonly Color ActorFillColor           = new(0.22f, 0.5f, 0.95f, 0.32f);
		private static readonly Color ActorOutlineColor        = new(0.42f, 0.68f, 1.0f, 0.92f);

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUILayout.Space();

			if (GUILayout.Button("Rebuild Grid")) {
				RebuildGrid((NavGrid)target);
			}
		}

		[DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
		private static void DrawNavGridGizmo(NavGrid navGrid, GizmoType gizmoType)
		{
			if (navGrid == null) {
				return;
			}

			DrawGrid(navGrid);
			DrawOccupiedCells(navGrid);
		}

		private static void DrawGrid(NavGrid navGrid)
		{
			Vector3[] cellCorners = new Vector3[4];
			Handles.color = GridColor;

			for (int y = 0; y < navGrid.Height; y++) {
				for (int x = 0; x < navGrid.Width; x++) {
					GetCellCorners(navGrid, x, y, cellCorners);
					Handles.DrawPolyLine(cellCorners[0], cellCorners[1], cellCorners[2], cellCorners[3], cellCorners[0]);
				}
			}
		}

		private static void DrawOccupiedCells(NavGrid navGrid)
		{
			if (navGrid.Nodes.Data == null || navGrid.Nodes.Data.Length != navGrid.NodeCount) {
				return;
			}

			Vector3[] cellCorners = new Vector3[4];

			for (int y = 0; y < navGrid.Height; y++) {
				for (int x = 0; x < navGrid.Width; x++) {
					NavCellOccupancy occupancy = navGrid.Nodes[x, y].Occupancy;
					if (occupancy.Type == NavCellOccupancyType.Empty) {
						continue;
					}

					GetCellCorners(navGrid, x, y, cellCorners);
					GetColors(occupancy.Type, out Color fillColor, out Color outlineColor);
					Handles.DrawSolidRectangleWithOutline(cellCorners, fillColor, outlineColor);
					Handles.Label(navGrid.GetCellWorldCenter(x, y), GetLabel(occupancy));
				}
			}
		}

		private static void GetCellCorners(NavGrid navGrid, int x, int y, Vector3[] corners)
		{
			Vector3 origin  = navGrid.GetCellWorldCorner(x, y);
			Vector3 right   = navGrid.transform.right;
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

		private static void GetColors(NavCellOccupancyType type, out Color fillColor, out Color outlineColor)
		{
			switch (type) {
				case NavCellOccupancyType.StaticProp:
					fillColor    = StaticPropFillColor;
					outlineColor = StaticPropOutlineColor;
					break;
				case NavCellOccupancyType.DestructibleProp:
					fillColor    = DestructibleFillColor;
					outlineColor = DestructibleOutlineColor;
					break;
				case NavCellOccupancyType.DecorativeDestructibleProp:
					fillColor    = DecorativeFillColor;
					outlineColor = DecorativeOutlineColor;
					break;
				case NavCellOccupancyType.Actor:
					fillColor    = ActorFillColor;
					outlineColor = ActorOutlineColor;
					break;
				default:
					fillColor    = Color.clear;
					outlineColor = Color.clear;
					break;
			}
		}

		private static string GetLabel(NavCellOccupancy occupancy)
		{
			return occupancy.Type switch {
				NavCellOccupancyType.StaticProp                 => "S",
				NavCellOccupancyType.DestructibleProp           => "D",
				NavCellOccupancyType.DecorativeDestructibleProp => "Dec",
				NavCellOccupancyType.Actor                      => "A",
				_                                               => string.Empty
			};
		}
	}
}
