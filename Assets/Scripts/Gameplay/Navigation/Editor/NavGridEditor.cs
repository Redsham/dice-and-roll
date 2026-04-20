using UnityEditor;
using UnityEngine;


namespace Gameplay.Navigation.Editor
{
	[CustomEditor(typeof(NavGrid))]
	public sealed class NavGridEditor : UnityEditor.Editor
	{
		// === Colors ===

		private static readonly Color GridColor                       = new(1f, 1f, 1f, 0.35f);
		private static readonly Color StaticObstacleFillColor         = new(0.72f, 0.24f, 0.18f, 0.45f);
		private static readonly Color StaticObstacleOutlineColor      = new(0.85f, 0.34f, 0.28f, 0.95f);
		private static readonly Color DestructibleObstacleFillColor   = new(0.95f, 0.56f, 0.16f, 0.38f);
		private static readonly Color DestructibleObstacleOutlineColor = new(1.0f, 0.72f, 0.28f, 0.95f);
		private static readonly Color CrushablePropFillColor          = new(0.82f, 0.78f, 0.18f, 0.2f);
		private static readonly Color CrushablePropOutlineColor       = new(0.98f, 0.92f, 0.35f, 0.9f);
		private static readonly Color ActorFillColor                  = new(0.22f, 0.5f, 0.95f, 0.32f);
		private static readonly Color ActorOutlineColor               = new(0.42f, 0.68f, 1.0f, 0.92f);
		
		private const float LABEL_VISIBILITY_PIXEL_THRESHOLD = 42f;

		private static readonly Vector3[] CellCorners = new Vector3[4];
		private static readonly Vector3[] InsetCellCorners = new Vector3[4];
		private static Vector3[] s_GridLinePoints;
		private static readonly GUIStyle LabelStyle = new(EditorStyles.boldLabel) {
			alignment = TextAnchor.MiddleCenter,
			normal = { textColor = Color.white }
		};

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
			Handles.color = GridColor;
			Vector3 origin = navGrid.transform.position;
			Vector3 right = navGrid.transform.right;
			Vector3 forward = navGrid.transform.forward;
			int lineCount = navGrid.Width + navGrid.Height + 2;
			int pointCount = lineCount * 2;
			if (s_GridLinePoints == null || s_GridLinePoints.Length != pointCount) {
				s_GridLinePoints = new Vector3[pointCount];
			}

			int pointIndex = 0;

			for (int x = 0; x <= navGrid.Width; x++) {
				Vector3 lineOrigin = origin + right * x;
				s_GridLinePoints[pointIndex++] = lineOrigin;
				s_GridLinePoints[pointIndex++] = lineOrigin + forward * navGrid.Height;
			}

			for (int y = 0; y <= navGrid.Height; y++) {
				Vector3 lineOrigin = origin + forward * y;
				s_GridLinePoints[pointIndex++] = lineOrigin;
				s_GridLinePoints[pointIndex++] = lineOrigin + right * navGrid.Width;
			}

			Handles.DrawLines(s_GridLinePoints);
		}

		private static void DrawOccupiedCells(NavGrid navGrid)
		{
			if (navGrid.Nodes.Data == null || navGrid.Nodes.Data.Length != navGrid.NodeCount) {
				return;
			}

			bool shouldDrawLabels = ShouldDrawLabels(navGrid);

			for (int y = 0; y < navGrid.Height; y++) {
				for (int x = 0; x < navGrid.Width; x++) {
					GetCellCorners(navGrid, x, y, CellCorners);
					DrawEntity(navGrid.Nodes[x, y].Tile, navGrid, x, y, CellCorners, 0.0f, shouldDrawLabels);
					DrawEntity(navGrid.Nodes[x, y].Actor, navGrid, x, y, CellCorners, 0.18f, shouldDrawLabels);
				}
			}
		}

		private static void DrawEntity(INavCellEntity entity, NavGrid navGrid, int x, int y, Vector3[] cellCorners, float inset, bool shouldDrawLabel)
		{
			if (entity == null) {
				return;
			}

			Vector3[] corners = inset <= 0.0f ? cellCorners : InsetCorners(cellCorners, inset, InsetCellCorners);
			GetColors(entity, out Color fillColor, out Color outlineColor);
			Handles.DrawSolidRectangleWithOutline(corners, fillColor, outlineColor);
			if (shouldDrawLabel) {
				Handles.Label(navGrid.GetCellWorldCenter(x, y), GetLabel(entity), LabelStyle);
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

		private static Vector3[] InsetCorners(Vector3[] corners, float inset, Vector3[] buffer)
		{
			Vector3 center = (corners[0] + corners[2]) * 0.5f;
			buffer[0] = Vector3.Lerp(corners[0], center, inset);
			buffer[1] = Vector3.Lerp(corners[1], center, inset);
			buffer[2] = Vector3.Lerp(corners[2], center, inset);
			buffer[3] = Vector3.Lerp(corners[3], center, inset);
			return buffer;
		}

		private static bool ShouldDrawLabels(NavGrid navGrid)
		{
			SceneView sceneView = SceneView.currentDrawingSceneView;
			if (sceneView == null || sceneView.camera == null) {
				return false;
			}

			Vector3 origin = navGrid.GetCellWorldCorner(0, 0);
			Vector2 a = HandleUtility.WorldToGUIPoint(origin);
			Vector2 b = HandleUtility.WorldToGUIPoint(origin + navGrid.transform.right);
			return (b - a).sqrMagnitude >= LABEL_VISIBILITY_PIXEL_THRESHOLD * LABEL_VISIBILITY_PIXEL_THRESHOLD;
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

			if (IsCrushableProp(entity)) {
				fillColor    = CrushablePropFillColor;
				outlineColor = CrushablePropOutlineColor;
				return;
			}

			if (IsDestructibleObstacle(entity)) {
				fillColor    = DestructibleObstacleFillColor;
				outlineColor = DestructibleObstacleOutlineColor;
				return;
			}

			fillColor    = StaticObstacleFillColor;
			outlineColor = StaticObstacleOutlineColor;
		}

		private static string GetLabel(INavCellEntity entity)
		{
			if (IsActor(entity)) {
				return "A";
			}

			if (IsCrushableProp(entity)) {
				return "C";
			}

			if (IsDestructibleObstacle(entity)) {
				return "DO";
			}

			return "SO";
		}

		// === Classification ===

		private static bool IsActor(INavCellEntity entity)
		{
			GameObject owner = entity.Owner;
			if (owner == null) {
				return false;
			}

			return owner.GetComponent("EnemyBehaviour") != null
				|| owner.GetComponent("DiceBehaviour") != null;
		}

		private static bool IsDestructibleObstacle(INavCellEntity entity)
		{
			GameObject owner = entity.Owner;
			return owner != null && owner.GetComponent("DestructibleObstacleTileBehaviour") != null;
		}

		private static bool IsCrushableProp(INavCellEntity entity)
		{
			GameObject owner = entity.Owner;
			return owner != null && owner.GetComponent("CrushablePropTileBehaviour") != null;
		}
	}
}
