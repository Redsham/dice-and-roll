using Gameplay.Navigation;
using Gameplay.Nodes.Contracts;
using Gameplay.Nodes.Models;


namespace Gameplay.Nodes.Authoring
{
	public sealed class DecorativeDestructibleNodeBehaviour : DestructiblePropNodeBehaviour, INodeActorEnterHandler
	{
		public override NavCellFlags Flags => IsAlive ? NavCellFlags.Walkable | NavCellFlags.Hittable : NavCellFlags.None;

		public void OnActorEnter(in NodeActorContext context)
		{
			ApplyDamage(new(context.Actor, context.Cell, int.MaxValue));
		}
	}
}
