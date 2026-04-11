namespace Gameplay.Nodes.Models
{
	public readonly struct NodeDamageResult
	{
		public int  ConsumedDamage { get; }
		public bool WasDestroyed   { get; }

		public NodeDamageResult(int consumedDamage, bool wasDestroyed)
		{
			ConsumedDamage = consumedDamage;
			WasDestroyed   = wasDestroyed;
		}
	}
}
