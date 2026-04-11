using Gameplay.Nodes.Models;


namespace Gameplay.Nodes.Contracts
{
	public interface INodeDamageHandler
	{
		NodeDamageResult ApplyDamage(in NodeDamageContext context);
	}
}