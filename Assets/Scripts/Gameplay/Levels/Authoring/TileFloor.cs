using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	public sealed class TileFloor : GridAlignedBehaviour
	{
		[field: SerializeField] public GridPositionAlignment Alignment { get; private set; } = GridPositionAlignment.Corner;

		public override Vector3 GetAlignedWorldPosition(NavGrid navGrid, Vector2Int gridPosition)
		{
			return navGrid.GetCellWorldPosition(gridPosition, Alignment);
		}

		protected override Vector2Int CalculateGridPosition(NavGrid navGrid)
		{
			return navGrid.GetCellCoordinates(transform.position, Alignment);
		}

		protected override void OnGridPositionChanged(Vector2Int gridPosition)
		{
			gameObject.name = $"Tile_{gridPosition.x}_{gridPosition.y}";
		}
	}
}
