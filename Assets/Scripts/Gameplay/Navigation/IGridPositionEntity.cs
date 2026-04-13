namespace Gameplay.Navigation
{
	// Shared authoring/runtime helper for scene objects whose position is derived
	// from a NavGrid cell and may need validation against grid bounds.
	public abstract class GridAlignedBehaviour : UnityEngine.MonoBehaviour
	{
		[field: UnityEngine.SerializeField] public UnityEngine.Vector2Int GridPosition { get; private set; }

		public bool   IsInNavGrid => m_IsInNavGrid;
		public string GridWarning => m_GridWarning;

		public bool SyncToGrid(NavGrid navGrid)
		{
			if (navGrid == null) {
				SetGridPosition(default);
				SetGridValidation(false, "NavGrid is not assigned.");
				return false;
			}

			SetGridPosition(CalculateGridPosition(navGrid));
			if (!navGrid.IsInBounds(GridPosition)) {
				SetGridValidation(false, $"{ValidationLabel} is outside NavGrid bounds. Calculated GridPosition: {GridPosition}.");
				return false;
			}

			AlignToGrid(navGrid, GridPosition);
			SetGridValidation(true, string.Empty);
			return true;
		}

		public void ClearGridValidation(string warning)
		{
			SetGridPosition(default);
			SetGridValidation(false, warning);
		}

		public virtual void SetGridPosition(UnityEngine.Vector2Int gridPosition)
		{
			GridPosition = gridPosition;
			OnGridPositionChanged(gridPosition);
		}

		public abstract UnityEngine.Vector3 GetAlignedWorldPosition(NavGrid navGrid, UnityEngine.Vector2Int gridPosition);

		protected abstract UnityEngine.Vector2Int CalculateGridPosition(NavGrid navGrid);

		protected virtual void AlignToGrid(NavGrid navGrid, UnityEngine.Vector2Int gridPosition)
		{
		}

		protected virtual void OnGridPositionChanged(UnityEngine.Vector2Int gridPosition)
		{
		}

		protected virtual string ValidationLabel => GetType().Name;

		private void SetGridValidation(bool isInNavGrid, string warning)
		{
			m_IsInNavGrid = isInNavGrid;
			m_GridWarning = warning;
		}

		[UnityEngine.SerializeField, UnityEngine.HideInInspector] private bool   m_IsInNavGrid = true;
		[UnityEngine.SerializeField, UnityEngine.HideInInspector] private string m_GridWarning;
	}
}
