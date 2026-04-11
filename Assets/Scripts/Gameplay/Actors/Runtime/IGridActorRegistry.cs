using UnityEngine;


namespace Gameplay.Actors.Runtime
{
	public interface IGridActorRegistry
	{
		// === State ===

		bool HasActors { get; }

		// === Lifecycle ===

		void Clear();
		void Register(IGridActor actor);
		void Unregister(IGridActor actor);
		void Move(IGridActor actor, Vector2Int from, Vector2Int to);

		// === Queries ===

		bool IsOccupied(Vector2Int cell);
		bool TryGet(Vector2Int cell, out IGridActor actor);
	}
}
