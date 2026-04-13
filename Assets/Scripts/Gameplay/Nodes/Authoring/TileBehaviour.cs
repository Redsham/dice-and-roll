using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Nodes.Authoring
{
	public abstract class TileBehaviour : GridAlignedBehaviour, INavCellEntity
	{
		[field: SerializeField] public GridPivot Pivot { get; private set; } = GridPivot.Corner;

		public         NavCellEntityLayer Layer => NavCellEntityLayer.Tile;
		public         GameObject   Owner   => gameObject;
		public         Vector2Int   Cell    => GridPosition;
		public virtual NavCellFlags Flags   => NavCellFlags.None;
		public virtual bool         IsAlive => true;

		private NavGrid m_NavGrid;

		public virtual void ResetRuntimeState()                               { }
		public virtual int  ApplyDamage(int damage, GameObject source = null) => 0;

		public void BindToGrid(NavGrid navGrid) => m_NavGrid = navGrid;
		public void ClearBoundGrid()            => m_NavGrid = null;

		public override    Vector3    GetAlignedWorldPosition(NavGrid navGrid, Vector2Int gridPosition) => navGrid.GetCellWorldPosition(gridPosition, Pivot);
		protected override Vector2Int CalculateGridPosition(NavGrid   navGrid) => navGrid.GetCellCoordinates(transform.position, Pivot);

		protected override string ValidationLabel => "Tile";

		protected void RemoveFromGrid() => m_NavGrid?.TryClearEntity(Cell, this);
	}
}
