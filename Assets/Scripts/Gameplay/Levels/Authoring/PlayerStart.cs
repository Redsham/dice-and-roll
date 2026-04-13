using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	public sealed class PlayerStart : GridAlignedBehaviour
	{
		public override Vector3 GetAlignedWorldPosition(NavGrid navGrid, Vector2Int gridPosition)
		{
			return navGrid.GetCellWorldCenter(gridPosition.x, gridPosition.y);
		}

		protected override Vector2Int CalculateGridPosition(NavGrid navGrid)
		{
			return navGrid.GetCellCoordinates(transform.position);
		}

		protected override void AlignToGrid(NavGrid navGrid, Vector2Int gridPosition)
		{
			transform.position = GetAlignedWorldPosition(navGrid, gridPosition);
		}
	}
}
