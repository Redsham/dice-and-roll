using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Presentation;
using Gameplay.Enemies.Runtime;
using Gameplay.Navigation;
using Gameplay.Player.Domain;
using TriInspector;
using UnityEngine;


namespace Gameplay.Enemies.Authoring
{
	public abstract class EnemyBehaviour : MonoBehaviour, IGridPositionEntity
	{
		// === Identity ===

		public abstract EnemyKind Kind { get; }

		// === Inspector ===

		[Title("Grid")]
		[field: SerializeField] public Vector2Int GridPosition { get;     private set; }
		[field: SerializeField] public RollDirection InitialFacing { get; private set; } = RollDirection.South;

		[Title("References")]
		[field: SerializeField, Required] public EnemyView View { get; private set; }

		[Title("Config")]
		[field: SerializeField, Required] public EnemyConfig Config { get; private set; }

		[Title("Debug"), ReadOnly]
		[SerializeField] private bool m_RuntimeBound;

		// === API ===

		public virtual NavCellOccupancy CreateOccupancy()
		{
			return new() {
				Type = NavCellOccupancyType.Actor
			};
		}

		public EnemyRuntimeHandle RuntimeHandle { get; private set; }

		public void BindRuntime(EnemyRuntimeHandle runtimeHandle)
		{
			RuntimeHandle  = runtimeHandle;
			m_RuntimeBound = runtimeHandle != null;
		}

		public void SetGridPosition(Vector2Int gridPosition)
		{
			GridPosition = gridPosition;
		}
	}
}
