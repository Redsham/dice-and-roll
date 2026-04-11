using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Enemies.Runtime
{
	public struct EnemyState
	{
		public Vector2Int    Position;
		public RollDirection Facing;
		public int           CurrentHealth;

		public static EnemyState Create(Vector2Int position, int maxHealth)
		{
			return new() {
				Position      = position,
				Facing        = RollDirection.South,
				CurrentHealth = maxHealth
			};
		}
	}
}