using Gameplay.Navigation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Nodes.Authoring
{
	public abstract class NodeBehaviour : MonoBehaviour
	{
		[Title("Grid")]
		[field: SerializeField] public Vector2Int GridPosition { get; private set; }

		public abstract NavCellOccupancy CreateOccupancy();

		public virtual void ResetRuntimeState()
		{
		}
	}
}
