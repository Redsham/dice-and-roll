using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using TriInspector;
using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	public sealed class LevelBehaviour : MonoBehaviour
	{
		[field: SerializeField, Required] public NavGrid     NavGrid     { get; private set; } = null;
		[field: SerializeField, Required] public PlayerStart PlayerStart { get; private set; } = null;
		[field: SerializeField]           public Transform   PropsRoot   { get; private set; } = null;

		public void Initialize()
		{
			PreviewNodesToGrid();
		}

		public NodeBehaviour[] GetNodes()
		{
			Transform nodeRoot = PropsRoot != null ? PropsRoot : transform;
			return nodeRoot.GetComponentsInChildren<NodeBehaviour>(true);
		}

		public void PreviewNodesToGrid()
		{
			NavGrid.RebuildGrid();
			NavGrid.ClearOccupancy();

			NodeBehaviour[] nodes = GetNodes();
			for (int i = 0; i < nodes.Length; i++) {
				NodeBehaviour node = nodes[i];
				node.ResetRuntimeState();
				NavGrid.TrySetOccupancy(node.GridPosition, node.CreateOccupancy());
			}
		}
	}
}
