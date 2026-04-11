using Gameplay.Nodes.Models;


namespace Gameplay.Nodes.Contracts
{
	public interface INodeActorLeaveHandler
	{
		void OnActorLeave(in NodeActorContext context);
	}
}
