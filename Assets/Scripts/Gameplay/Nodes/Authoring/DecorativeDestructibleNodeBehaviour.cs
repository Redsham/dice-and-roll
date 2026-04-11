using Gameplay.Navigation;
using Gameplay.Nodes.Contracts;
using Gameplay.Nodes.Models;
using UnityEngine;


namespace Gameplay.Nodes.Authoring
{
	public sealed class DecorativeDestructibleNodeBehaviour : DestructiblePropNodeBehaviour, INodeActorEnterHandler
	{
		public override NavCellOccupancy CreateOccupancy()
		{
			NavCellOccupancy occupancy = base.CreateOccupancy();
			if (occupancy.Type == NavCellOccupancyType.Empty) {
				return occupancy;
			}

			occupancy.Type = NavCellOccupancyType.DecorativeDestructibleProp;
			return occupancy;
		}

		public void OnActorEnter(in NodeActorContext context)
		{
			ApplyDamage(new NodeDamageContext(context.Actor, context.Cell, int.MaxValue));
		}
	}
}
