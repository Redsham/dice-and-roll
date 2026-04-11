using TriInspector;
using UnityEngine;


namespace Gameplay.Enemies.Configs
{
	[CreateAssetMenu(fileName = "PawnEnemyConfig", menuName = "Game/Gameplay/Enemies/Pawn Enemy Config")]
	public sealed class PawnEnemyConfig : EnemyConfig
	{
		[Title("Pawn")]
		[field: SerializeField, Min(1)] public int ShootRange { get; private set; } = 2;
		[field: SerializeField, Min(1)] public int ShootDamage { get; private set; } = 1;
	}
}
