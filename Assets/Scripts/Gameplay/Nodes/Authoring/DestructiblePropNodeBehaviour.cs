using Gameplay.Navigation;
using Gameplay.Nodes.Contracts;
using Gameplay.Nodes.Models;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;


namespace Gameplay.Nodes.Authoring
{
	public class DestructiblePropNodeBehaviour : TileBehaviour, INodeDamageHandler
	{
		[Title("Destructible")]
		[SerializeField, Min(1)] private int m_HitPoints = 1;
		[SerializeField] private UnityEvent m_OnDestroyed;

		private int  m_CurrentHitPoints;
		private bool m_IsDestroyed;

		public override void ResetRuntimeState()
		{
			m_CurrentHitPoints = Mathf.Max(1, m_HitPoints);
			m_IsDestroyed      = false;
		}

		public override NavCellOccupancy CreateOccupancy()
		{
			if (m_IsDestroyed) {
				return NavCellOccupancy.Empty;
			}

			if (m_CurrentHitPoints <= 0) {
				m_CurrentHitPoints = Mathf.Max(1, m_HitPoints);
			}

			return new() {
				Type = NavCellOccupancyType.DestructibleProp
			};
		}

		public virtual NodeDamageResult ApplyDamage(in NodeDamageContext context)
		{
			if (m_IsDestroyed || context.RequestedDamage <= 0) {
				return default;
			}

			int consumedDamage = Mathf.Min(context.RequestedDamage, m_CurrentHitPoints);
			m_CurrentHitPoints -= consumedDamage;
			if (m_CurrentHitPoints <= 0) {
				DestroyNode();
				return new(consumedDamage, true);
			}

			return new(consumedDamage, false);
		}

		protected void DestroyNode()
		{
			if (m_IsDestroyed) {
				return;
			}

			m_IsDestroyed      = true;
			m_CurrentHitPoints = 0;
			m_OnDestroyed?.Invoke();
		}
	}
}
