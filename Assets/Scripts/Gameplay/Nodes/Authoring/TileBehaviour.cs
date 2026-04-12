using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Nodes.Authoring
{
	public abstract class TileBehaviour : MonoBehaviour, IGridPositionEntity, INavCellEntity
	{
		[field: SerializeField] public Vector2Int GridPosition { get; private set; }
		[field: SerializeField] public GridPositionAlignment Alignment { get; private set; } = GridPositionAlignment.Corner;

		public GameObject Owner => gameObject;
		public Vector2Int Cell => GridPosition;
		public virtual NavCellFlags Flags => NavCellFlags.None;
		public virtual bool IsAlive => true;

		public bool   IsInNavGrid => m_IsInNavGrid;
		public string GridWarning => m_GridWarning;

		public virtual void ResetRuntimeState()
		{
		}

		public virtual int ApplyDamage(int damage, GameObject source = null)
		{
			return 0;
		}

		public bool SyncToGrid(NavGrid navGrid)
		{
			if (navGrid == null) {
				SetGridPosition(default);
				SetGridValidation(false, "NavGrid is not assigned.");
				return false;
			}

			SetGridPosition(navGrid.GetCellCoordinates(transform.position, Alignment));

			bool isInBounds = navGrid.IsInBounds(GridPosition);
			if (isInBounds) {
				SetGridValidation(true, string.Empty);
				return true;
			}

			SetGridValidation(false, $"Tile is outside NavGrid bounds. Calculated GridPosition: {GridPosition}.");
			return false;
		}

		public void ClearGridValidation(string warning)
		{
			SetGridPosition(default);
			SetGridValidation(false, warning);
		}

		public void SetGridPosition(Vector2Int gridPosition)
		{
			GridPosition = gridPosition;
		}

		public Vector3 GetAlignedWorldPosition(NavGrid navGrid, Vector2Int gridPosition)
		{
			return navGrid.GetCellWorldPosition(gridPosition, Alignment);
		}

		private void SetGridValidation(bool isInNavGrid, string warning)
		{
			m_IsInNavGrid = isInNavGrid;
			m_GridWarning = warning;
		}

		[SerializeField, HideInInspector] private bool   m_IsInNavGrid = true;
		[SerializeField, HideInInspector] private string m_GridWarning;
	}
}
