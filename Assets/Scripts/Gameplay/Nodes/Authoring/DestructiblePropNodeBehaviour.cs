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

		public override NavCellFlags Flags => IsAlive ? NavCellFlags.Hittable : NavCellFlags.None;
		public override bool IsAlive => !m_IsDestroyed;

		public override void ResetRuntimeState()
		{
			m_CurrentHitPoints = Mathf.Max(1, m_HitPoints);
			m_IsDestroyed      = false;
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

		public override int ApplyDamage(int damage, GameObject source = null)
		{
			return ApplyDamage(new(source, Cell, damage)).ConsumedDamage;
		}

		protected void DestroyNode()
		{
			if (m_IsDestroyed) {
				return;
			}

			m_IsDestroyed      = true;
			m_CurrentHitPoints = 0;
			RemoveFromGrid();
			m_OnDestroyed?.Invoke();
		}
	}
}
