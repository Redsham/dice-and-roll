using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public sealed class CombatResolverService : ICombatResolverService
	{
		// === Dependencies ===

		private readonly INavEntityService m_NavEntityService;

		public CombatResolverService(INavEntityService navEntityService)
		{
			m_NavEntityService = navEntityService;
		}

		// === Lifecycle ===

		public void Clear()
		{
		}

		public void RegisterActor(IGridActor actor)
		{
			if (actor == null) {
				return;
			}

			m_NavEntityService.TrySetEntity(actor.Cell, actor);
		}

		public void UnregisterActor(IGridActor actor)
		{
			if (actor == null) {
				return;
			}

			m_NavEntityService.TryClearEntity(actor.Cell, actor);
		}

		public void MoveActor(IGridActor actor, Vector2Int from, Vector2Int to)
		{
			if (actor == null) {
				return;
			}

			m_NavEntityService.TryMoveEntity(actor, from, to);
		}

		// === Queries ===

		public int ApplyDamage(INavCellEntity entity, int damage, GameObject source = null)
		{
			if (entity == null || damage <= 0) {
				return 0;
			}

			int consumedDamage = entity.ApplyDamage(damage, source);
			if (!entity.IsAlive) {
				m_NavEntityService.TryClearEntity(entity.Cell, entity);
			}

			return consumedDamage;
		}

		public int ApplyDamage(Vector2Int cell, int damage, GameObject source = null)
		{
			return m_NavEntityService.TryGetEntity(cell, out INavCellEntity entity)
				? ApplyDamage(entity, damage, source)
				: 0;
		}
	}
}
