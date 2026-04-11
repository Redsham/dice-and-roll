using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Enemies.Runtime
{
	public enum EnemyTurnActionType
	{
		None = 0,
		Move = 1,
		Rotate = 2,
		Shoot = 3,
		Wait = 4
	}

	public readonly struct EnemyTurnAction
	{
		public EnemyTurnActionType Type { get; }
		public RollDirection Direction { get; }
		public Vector2Int TargetCell { get; }

		private EnemyTurnAction(EnemyTurnActionType type, RollDirection direction, Vector2Int targetCell)
		{
			Type = type;
			Direction = direction;
			TargetCell = targetCell;
		}

		public static EnemyTurnAction None() => new(EnemyTurnActionType.None, default, default);
		public static EnemyTurnAction Wait() => new(EnemyTurnActionType.Wait, default, default);
		public static EnemyTurnAction Move(RollDirection direction) => new(EnemyTurnActionType.Move, direction, default);
		public static EnemyTurnAction Rotate(RollDirection direction) => new(EnemyTurnActionType.Rotate, direction, default);
		public static EnemyTurnAction Shoot(Vector2Int targetCell) => new(EnemyTurnActionType.Shoot, default, targetCell);
	}
}
