namespace Gameplay.Nodes.Models
{
	public readonly struct NodeProjectileImpactInfo
	{
		public int  ConsumedDamage     { get; }
		public bool StopsProjectile    { get; }
		public bool CanApplyDamage     { get; }

		public NodeProjectileImpactInfo(int consumedDamage, bool stopsProjectile, bool canApplyDamage)
		{
			ConsumedDamage  = consumedDamage;
			StopsProjectile = stopsProjectile;
			CanApplyDamage  = canApplyDamage;
		}
	}
}
