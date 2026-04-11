using TriInspector;
using UnityEngine;


namespace Gameplay.Enemies.Configs
{
	[CreateAssetMenu(fileName = "MortarEnemyConfig", menuName = "Game/Gameplay/Enemies/Mortar Enemy Config")]
	public sealed class MortarEnemyConfig : EnemyConfig
	{
		[Title("Mortar")]
		[field: SerializeField, Min(1)] public int BombardmentIntervalTurns { get; private set; } = 3;
		[field: SerializeField, Min(1)] public int BombardmentRadius { get; private set; } = 2;
		[field: SerializeField, Min(1)] public int BombardmentDamage { get; private set; } = 3;
		[field: SerializeField, Min(1)] public int PreferredDistance { get; private set; } = 4;
	}
}
