using Gameplay.Navigation;
using TriInspector;


namespace Gameplay.Nodes.Authoring
{
	public sealed class StaticPropNodeBehaviour : NodeBehaviour
	{
		[Title("Static Prop")]
		[ShowInInspector, ReadOnly]
		private NavCellOccupancyType PreviewType => NavCellOccupancyType.StaticProp;

		public override NavCellOccupancy CreateOccupancy()
		{
			return new NavCellOccupancy {
				Type = NavCellOccupancyType.StaticProp
			};
		}
	}
}
