using UnityEngine;


namespace Gameplay.Nodes.Models
{
	public readonly struct NodeDamageContext
	{
		public GameObject Source          { get; }
		public Vector2Int Cell            { get; }
		public int        RequestedDamage { get; }

		public NodeDamageContext(GameObject source, Vector2Int cell, int requestedDamage)
		{
			Source          = source;
			Cell            = cell;
			RequestedDamage = requestedDamage;
		}
	}
}