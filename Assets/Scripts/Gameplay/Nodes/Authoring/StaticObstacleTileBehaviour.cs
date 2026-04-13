using Gameplay.Navigation;
using TriInspector;


namespace Gameplay.Nodes.Authoring
{
	public class StaticObstacleTileBehaviour : TileBehaviour
	{
		// === Inspector ===

		[Title("Static Obstacle")]
		[ShowInInspector, ReadOnly]
		private NavCellFlags PreviewFlags => Flags;

		// === Navigation ===

		public override NavCellFlags Flags => NavCellFlags.BlocksMovement | NavCellFlags.BlocksTrace;
	}
}
