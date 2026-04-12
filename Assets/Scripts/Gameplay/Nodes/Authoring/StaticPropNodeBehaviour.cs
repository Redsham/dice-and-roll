using Gameplay.Navigation;
using TriInspector;


namespace Gameplay.Nodes.Authoring
{
	public sealed class StaticPropNodeBehaviour : TileBehaviour
	{
		[Title("Static Prop")]
		[ShowInInspector, ReadOnly]
		private NavCellFlags PreviewFlags => Flags;
	}
}
