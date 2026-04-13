using Gameplay.Navigation;
using Gameplay.Nodes.Contracts;
using Gameplay.Nodes.Models;
using TriInspector;


namespace Gameplay.Nodes.Authoring
{
	public class CrushablePropTileBehaviour : DestroyableTileBehaviour, INodeActorEnterHandler
	{
		// === Inspector ===

		[Title("Crushable Prop")]
		[ShowInInspector, ReadOnly]
		private NavCellFlags PreviewFlags => Flags;

		// === Navigation ===

		public override NavCellFlags Flags => NavCellFlags.None;

		// === Events ===

		public void OnActorEnter(in NodeActorContext context)
		{
			DestroyTile();
		}
	}
}
