using UnityEngine;
using UnityEngine.UI;


namespace UI.Tooltips
{
	[DisallowMultipleComponent, RequireComponent(typeof(CanvasRenderer))]
	public sealed class UILine : MaskableGraphic
	{
		[SerializeField, Min(1.0f)] private float m_Thickness = 2.0f;

		private Vector2 m_Start;
		private Vector2 m_End;

		public void SetPoints(Vector2 start, Vector2 end)
		{
			m_Start = start;
			m_End   = end;
			SetVerticesDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			Vector2 direction = m_End - m_Start;
			if (direction.sqrMagnitude <= Mathf.Epsilon) {
				return;
			}

			Vector2 normal = new(-direction.y, direction.x);
			normal.Normalize();
			normal *= m_Thickness * 0.5f;

			UIVertex vertex = UIVertex.simpleVert;
			vertex.color = color;

			vertex.position = m_Start - normal;
			vh.AddVert(vertex);

			vertex.position = m_Start + normal;
			vh.AddVert(vertex);

			vertex.position = m_End + normal;
			vh.AddVert(vertex);

			vertex.position = m_End - normal;
			vh.AddVert(vertex);

			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}
	}
}
