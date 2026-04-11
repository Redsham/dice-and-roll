using Gameplay.Enemies.Configs;


namespace Gameplay.Enemies.Authoring
{
	public sealed class PawnEnemyBehaviour : EnemyBehaviour
	{
		public override Runtime.EnemyKind Kind => Runtime.EnemyKind.Pawn;
		public new PawnEnemyConfig Config => (PawnEnemyConfig)base.Config;
	}
}
