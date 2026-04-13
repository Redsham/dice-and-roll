using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Nodes.Authoring
{
	public abstract class TileBehaviour : GridAlignedBehaviour, INavCellEntity
	{
		[field: SerializeField] public GridPositionAlignment Alignment { get; private set; } = GridPositionAlignment.Corner;

		public GameObject Owner => gameObject;
		public Vector2Int Cell => GridPosition;
		public virtual NavCellFlags Flags => NavCellFlags.None;
		public virtual bool IsAlive => true;

		public virtual void ResetRuntimeState()
		{
		}

		public virtual int ApplyDamage(int damage, GameObject source = null)
		{
			return 0;
		}

		public void BindToGrid(NavGrid navGrid)
		{
			m_NavGrid = navGrid;
		}

		public void ClearBoundGrid()
		{
			m_NavGrid = null;
		}

		public override Vector3 GetAlignedWorldPosition(NavGrid navGrid, Vector2Int gridPosition)
		{
			return navGrid.GetCellWorldPosition(gridPosition, Alignment);
		}

		protected override Vector2Int CalculateGridPosition(NavGrid navGrid)
		{
			return navGrid.GetCellCoordinates(transform.position, Alignment);
		}

		protected override string ValidationLabel => "Tile";

		protected void RemoveFromGrid()
		{
			m_NavGrid?.TryClearEntity(Cell, this);
		}
		private NavGrid m_NavGrid;
	}
}
