using Gameplay.Navigation;
using Gameplay.Nodes.Models;
using Gameplay.Nodes.Runtime;
using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public sealed class CombatResolverService : ICombatResolverService
	{
		// === Dependencies ===

		private readonly IGridActorRegistry m_ActorRegistry;
		private readonly ILevelNodeService  m_LevelNodeService;

		public CombatResolverService(IGridActorRegistry actorRegistry, ILevelNodeService levelNodeService)
		{
			m_ActorRegistry = actorRegistry;
			m_LevelNodeService = levelNodeService;
		}

		// === Lifecycle ===

		public void Clear()
		{
			m_ActorRegistry.Clear();
		}

		public void RegisterActor(IGridActor actor)
		{
			m_ActorRegistry.Register(actor);
		}

		public void UnregisterActor(IGridActor actor)
		{
			m_ActorRegistry.Unregister(actor);
		}

		public void MoveActor(IGridActor actor, Vector2Int from, Vector2Int to)
		{
			m_ActorRegistry.Move(actor, from, to);
		}

		// === Queries ===

		public bool IsCellOccupiedByActor(Vector2Int cell)
		{
			return m_ActorRegistry.IsOccupied(cell);
		}

		public NodeProjectileImpactInfo PreviewProjectileImpact(Vector2Int cell, int incomingDamage, out NavCellOccupancy occupancy)
		{
			if (m_ActorRegistry.TryGet(cell, out IGridActor actor)) {
				occupancy = actor.Occupancy;
				return actor.PreviewProjectileImpact(incomingDamage);
			}

			return m_LevelNodeService.PreviewProjectileImpact(cell, incomingDamage, out occupancy);
		}

		public int ApplyDamage(Vector2Int cell, int damage, GameObject source = null)
		{
			if (damage <= 0) {
				return 0;
			}

			if (m_ActorRegistry.TryGet(cell, out IGridActor actor)) {
				int consumedDamage = actor.ApplyDamage(damage, source);
				if (!actor.IsAlive) {
					m_ActorRegistry.Unregister(actor);
				}

				return consumedDamage;
			}

			return m_LevelNodeService.ApplyDamage(cell, damage, source);
		}
	}
}
