using Gameplay.Nodes.Models;


namespace Gameplay.Nodes.Contracts
{
	public interface INodeProjectileImpactHandler
	{
		NodeProjectileImpactInfo PreviewProjectileImpact(int incomingDamage);
	}
}