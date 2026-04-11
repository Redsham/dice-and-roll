using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Runtime;


namespace Gameplay.Enemies.Authoring
{
	public sealed class PawnEnemyBehaviour : EnemyBehaviour
	{
		public override EnemyKind       Kind   => EnemyKind.Pawn;
		public new      PawnEnemyConfig Config => (PawnEnemyConfig)base.Config;
	}
}