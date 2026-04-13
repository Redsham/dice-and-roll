using Gameplay.Navigation;
using Gameplay.Nodes.Contracts;
using Gameplay.Nodes.Models;
using TriInspector;
using UnityEngine;


namespace Gameplay.Nodes.Authoring
{
	public class DestructibleObstacleTileBehaviour : DestroyableTileBehaviour, INodeDamageHandler
	{
		// === Inspector ===

		[Title("Destructible Obstacle")]
		[SerializeField, Min(1)] private int m_HitPoints = 1;

		// === State ===

		private int m_CurrentHitPoints;

		// === Navigation ===

		public override NavCellFlags Flags => IsAlive
			? NavCellFlags.BlocksMovement | NavCellFlags.BlocksTrace
			: NavCellFlags.None;

		// === Lifecycle ===

		protected override void OnResetRuntimeState()
		{
			m_CurrentHitPoints = Mathf.Max(1, m_HitPoints);
		}

		public virtual NodeDamageResult ApplyDamage(in NodeDamageContext context)
		{
			if (!IsAlive || context.RequestedDamage <= 0) {
				return default;
			}

			int consumedDamage = Mathf.Min(context.RequestedDamage, m_CurrentHitPoints);
			m_CurrentHitPoints -= consumedDamage;
			if (m_CurrentHitPoints <= 0) {
				DestroyTile();
				return new(consumedDamage, true);
			}

			return new(consumedDamage, false);
		}

		public override int ApplyDamage(int damage, GameObject source = null)
		{
			return ApplyDamage(new(source, Cell, damage)).ConsumedDamage;
		}
	}
}
