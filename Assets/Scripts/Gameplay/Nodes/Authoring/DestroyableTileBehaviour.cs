using TriInspector;
using UnityEngine;
using UnityEngine.Events;


namespace Gameplay.Nodes.Authoring
{
	public abstract class DestroyableTileBehaviour : TileBehaviour
	{
		// === Inspector ===

		[Title("Destroyed")]
		[SerializeField] private UnityEvent m_OnDestroyed;

		// === State ===

		private bool m_IsDestroyed;

		// === Navigation ===

		public override bool IsAlive => !m_IsDestroyed;

		// === Lifecycle ===

		public override void ResetRuntimeState()
		{
			m_IsDestroyed = false;
			OnResetRuntimeState();
		}

		// === Hooks ===

		protected virtual void OnResetRuntimeState() { }

		// === Helpers ===

		protected void DestroyTile()
		{
			if (m_IsDestroyed) {
				return;
			}

			m_IsDestroyed = true;
			RemoveFromGrid();
			m_OnDestroyed?.Invoke();
		}
	}
}
