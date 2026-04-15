using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Enemies.Presentation
{
	public sealed class MortarAimMarkerView : MonoBehaviour
	{
		// === Inspector ===

		[SerializeField] private GameObject m_Root;

		// === Runtime ===

		private bool m_IsDetached;

		private GameObject Root => m_Root != null ? m_Root : gameObject;

		// === API ===

		public void Hide()
		{
			Root.SetActive(false);
		}

		public void Show(Vector2Int cell, GridBasis basis)
		{
			EnsureDetached();
			transform.position = basis.GetCellCenter(cell) + new Vector3(0, 0.5f, 0);
			Root.SetActive(true);
		}

		// === Helpers ===

		private void EnsureDetached()
		{
			if (m_IsDetached || !Application.isPlaying) {
				return;
			}

			// The marker is authored as a child of the mortar prefab, but once it starts
			// previewing a strike it should live in world-space and stay on the targeted cell.
			transform.SetParent(null, true);
			m_IsDetached = true;
		}
	}
}