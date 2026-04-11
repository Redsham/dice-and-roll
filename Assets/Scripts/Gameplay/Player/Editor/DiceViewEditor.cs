using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation;
using UnityEditor;
using UnityEngine;


namespace Gameplay.Player.Editor
{
	[CustomEditor(typeof(DiceView))]
	public sealed class DiceViewEditor : UnityEditor.Editor
	{
		private static readonly DiceFace[] Faces = {
			DiceFace.Top,
			DiceFace.Bottom,
			DiceFace.Left,
			DiceFace.Right,
			DiceFace.Forward,
			DiceFace.Backward
		};

		private const float LabelOffsetFromFace = 2.0f;

		private static readonly Color LineColor  = new(1.0f, 0.75f, 0.2f, 0.9f);
		private static readonly Color LabelColor = new(1.0f, 0.95f, 0.65f, 1.0f);

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
		}

		private void OnSceneGUI()
		{
			DiceView diceView = (DiceView)target;
			if (diceView == null) {
				return;
			}

			Transform transform = diceView.transform;
			Vector3   center    = GetVisualCenter(diceView);
			float     distance  = GetDistanceToFaceCenter(diceView, center) + LabelOffsetFromFace;

			GUIStyle labelStyle = CreateLabelStyle();
			using (new Handles.DrawingScope(LineColor))
			{
				for (int i = 0; i < Faces.Length; i++) {
					DiceFace face        = Faces[i];
					Vector3  worldNormal = transform.TransformDirection(GetLocalNormal(face));
					Vector3  labelPoint  = center + worldNormal * distance;
					int      faceValue   = DiceOrientation.Default.GetFaceValue(face);

					Handles.DrawLine(center, labelPoint);
					Handles.SphereHandleCap(0, labelPoint, Quaternion.identity, HandleUtility.GetHandleSize(labelPoint) * 0.06f, EventType.Repaint);
					Handles.Label(labelPoint, faceValue.ToString(), labelStyle);
				}
			}
		}

		private static GUIStyle CreateLabelStyle()
		{
			GUIStyle style = new(EditorStyles.boldLabel) {
				alignment = TextAnchor.MiddleCenter,fontSize = 24
			};
			style.normal.textColor = LabelColor;
			return style;
		}

		private static Vector3 GetVisualCenter(DiceView diceView)
		{
			Renderer[] renderers = GetDiceRenderers(diceView);
			if (renderers.Length == 0) {
				return diceView.transform.position;
			}

			Bounds bounds = renderers[0].bounds;
			for (int i = 1; i < renderers.Length; i++) {
				bounds.Encapsulate(renderers[i].bounds);
			}

			return bounds.center;
		}

		private static float GetDistanceToFaceCenter(DiceView diceView, Vector3 center)
		{
			Renderer[] renderers = GetDiceRenderers(diceView);
			if (renderers.Length == 0) {
				return 0.0f;
			}

			Bounds bounds = renderers[0].bounds;
			for (int i = 1; i < renderers.Length; i++) {
				bounds.Encapsulate(renderers[i].bounds);
			}

			return Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
		}

		private static Renderer[] GetDiceRenderers(DiceView diceView)
		{
			Renderer[] allRenderers = diceView.GetComponentsInChildren<Renderer>();
			int        count        = 0;

			for (int i = 0; i < allRenderers.Length; i++) {
				if (allRenderers[i] == null || allRenderers[i] is ParticleSystemRenderer) {
					continue;
				}

				count++;
			}

			if (count == 0) {
				return new Renderer[0];
			}

			Renderer[] filteredRenderers = new Renderer[count];
			int        index             = 0;

			for (int i = 0; i < allRenderers.Length; i++) {
				Renderer renderer = allRenderers[i];
				if (renderer == null || renderer is ParticleSystemRenderer) {
					continue;
				}

				filteredRenderers[index++] = renderer;
			}

			return filteredRenderers;
		}

		private static Vector3 GetLocalNormal(DiceFace face)
		{
			return face switch {
				DiceFace.Top      => Vector3.up,
				DiceFace.Bottom   => Vector3.down,
				DiceFace.Left     => Vector3.left,
				DiceFace.Right    => Vector3.right,
				DiceFace.Forward  => Vector3.forward,
				DiceFace.Backward => Vector3.back,
				_                 => Vector3.zero
			};
		}
	}
}
