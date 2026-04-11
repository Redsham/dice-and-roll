using System.Collections.Generic;
using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public sealed class GridActorRegistry : IGridActorRegistry
	{
		// === Runtime ===

		private readonly Dictionary<Vector2Int, IGridActor> m_ActorsByCell = new();

		// === State ===

		public bool HasActors => m_ActorsByCell.Count > 0;

		// === Lifecycle ===

		public void Clear()
		{
			m_ActorsByCell.Clear();
		}

		public void Register(IGridActor actor)
		{
			if (actor == null) {
				return;
			}

			m_ActorsByCell[actor.Cell] = actor;
		}

		public void Unregister(IGridActor actor)
		{
			if (actor == null) {
				return;
			}

			if (m_ActorsByCell.TryGetValue(actor.Cell, out IGridActor currentActor) && ReferenceEquals(currentActor, actor)) {
				m_ActorsByCell.Remove(actor.Cell);
			}
		}

		public void Move(IGridActor actor, Vector2Int from, Vector2Int to)
		{
			if (actor == null) {
				return;
			}

			if (m_ActorsByCell.TryGetValue(from, out IGridActor currentActor) && ReferenceEquals(currentActor, actor)) {
				m_ActorsByCell.Remove(from);
			}

			m_ActorsByCell[to] = actor;
		}

		// === Queries ===

		public bool IsOccupied(Vector2Int cell)
		{
			return m_ActorsByCell.TryGetValue(cell, out IGridActor actor) && actor != null && actor.IsAlive;
		}

		public bool TryGet(Vector2Int cell, out IGridActor actor)
		{
			if (m_ActorsByCell.TryGetValue(cell, out actor) && actor != null && actor.IsAlive) {
				return true;
			}

			actor = null;
			return false;
		}
	}
}