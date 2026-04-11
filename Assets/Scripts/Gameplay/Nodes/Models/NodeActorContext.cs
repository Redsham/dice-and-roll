using UnityEngine;


namespace Gameplay.Nodes.Models
{
	public readonly struct NodeActorContext
	{
		public GameObject Actor { get; }
		public Vector2Int Cell  { get; }

		public NodeActorContext(GameObject actor, Vector2Int cell)
		{
			Actor = actor;
			Cell  = cell;
		}
	}
}