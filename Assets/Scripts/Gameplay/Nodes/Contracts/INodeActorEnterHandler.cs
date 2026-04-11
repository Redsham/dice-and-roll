using Gameplay.Nodes.Models;


namespace Gameplay.Nodes.Contracts
{
	public interface INodeActorEnterHandler
	{
		void OnActorEnter(in NodeActorContext context);
	}
}