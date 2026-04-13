using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	public sealed class PlayerStart : MonoBehaviour
	{
		[field: SerializeField] public Vector2Int GridPosition { get; private set; }

		public bool   IsInNavGrid => m_IsInNavGrid;
		public string GridWarning => m_GridWarning;

		public bool SyncToGrid(NavGrid navGrid)
		{
			if (navGrid == null) {
				SetGridPosition(default);
				SetValidation(false, "NavGrid is not assigned.");
				return false;
			}

			SetGridPosition(navGrid.GetCellCoordinates(transform.position));

			if (!navGrid.IsInBounds(GridPosition)) {
				SetValidation(false, $"PlayerStart is outside NavGrid bounds. Calculated GridPosition: {GridPosition}.");
				return false;
			}

			transform.position = navGrid.GetCellWorldCenter(GridPosition.x, GridPosition.y);
			SetValidation(true, string.Empty);
			return true;
		}

		public void ClearGridValidation(string warning)
		{
			SetGridPosition(default);
			SetValidation(false, warning);
		}

		public void SetGridPosition(Vector2Int gridPosition)
		{
			GridPosition = gridPosition;
		}

		private void SetValidation(bool isInNavGrid, string warning)
		{
			m_IsInNavGrid = isInNavGrid;
			m_GridWarning = warning;
		}

		[SerializeField, HideInInspector] private bool   m_IsInNavGrid = true;
		[SerializeField, HideInInspector] private string m_GridWarning;
	}
}
