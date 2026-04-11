using UnityEngine;


namespace Gameplay.Navigation
{
	public interface IGridPositionEntity
	{
		Vector2Int GridPosition { get; }

		void SetGridPosition(Vector2Int gridPosition);
	}
}
