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
					GetCellCorners(navGrid, x, y, cellCorners);
					DrawEntity(navGrid.Nodes[x, y].Tile, navGrid, x, y, cellCorners, 0.0f);
					DrawEntity(navGrid.Nodes[x, y].Actor, navGrid, x, y, cellCorners, 0.18f);
				}
			}
		}

		private static void DrawEntity(INavCellEntity entity, NavGrid navGrid, int x, int y, Vector3[] cellCorners, float inset)
		{
			if (entity == null) {
				return;
			}

			Vector3[] corners = inset <= 0.0f ? cellCorners : InsetCorners(cellCorners, inset);
			GetColors(entity, out Color fillColor, out Color outlineColor);
			Handles.DrawSolidRectangleWithOutline(corners, fillColor, outlineColor);
			Handles.Label(navGrid.GetCellWorldCenter(x, y), GetLabel(entity));
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

		private static Vector3[] InsetCorners(Vector3[] corners, float inset)
		{
			Vector3 center = (corners[0] + corners[2]) * 0.5f;
			return new[] {
				Vector3.Lerp(corners[0], center, inset),
				Vector3.Lerp(corners[1], center, inset),
				Vector3.Lerp(corners[2], center, inset),
				Vector3.Lerp(corners[3], center, inset)
			};
		}

		private static void RebuildGrid(NavGrid navGrid)
		{
			Undo.RecordObject(navGrid, "Rebuild Navigation Grid");
			navGrid.RebuildGrid();
			EditorUtility.SetDirty(navGrid);
			SceneView.RepaintAll();
		}

		private static void GetColors(INavCellEntity entity, out Color fillColor, out Color outlineColor)
		{
			if (IsActor(entity)) {
				fillColor    = ActorFillColor;
				outlineColor = ActorOutlineColor;
				return;
			}

			if (entity.Flags.HasFlag(NavCellFlags.Walkable) && entity.Flags.HasFlag(NavCellFlags.Hittable)) {
				fillColor    = DecorativeFillColor;
				outlineColor = DecorativeOutlineColor;
				return;
			}

			if (entity.Flags.HasFlag(NavCellFlags.Hittable)) {
				fillColor    = DestructibleFillColor;
				outlineColor = DestructibleOutlineColor;
				return;
			}

			fillColor    = StaticPropFillColor;
			outlineColor = StaticPropOutlineColor;
		}

		private static string GetLabel(INavCellEntity entity)
		{
			if (IsActor(entity)) {
				return "A";
			}

			if (entity.Flags.HasFlag(NavCellFlags.Walkable) && entity.Flags.HasFlag(NavCellFlags.Hittable)) {
				return "Dec";
			}

			if (entity.Flags.HasFlag(NavCellFlags.Hittable)) {
				return "D";
			}

			return "S";
		}

		private static bool IsActor(INavCellEntity entity)
		{
			GameObject owner = entity.Owner;
			if (owner == null) {
				return false;
			}

			return owner.GetComponent("EnemyBehaviour") != null
				|| owner.GetComponent("DiceBehaviour") != null;
		}
	}
}
