using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Flow.Input
{
	public enum PlayerTurnCommandType
	{
		Move,
		Shoot
	}

	public readonly struct PlayerTurnCommand
	{
		public PlayerTurnCommandType Type          { get; }
		public RollDirection         MoveDirection { get; }
		public Vector3               AimPoint      { get; }

		private PlayerTurnCommand(PlayerTurnCommandType type, RollDirection moveDirection, Vector3 aimPoint)
		{
			Type          = type;
			MoveDirection = moveDirection;
			AimPoint      = aimPoint;
		}

		public static PlayerTurnCommand Move(RollDirection direction)
		{
			return new PlayerTurnCommand(PlayerTurnCommandType.Move, direction, default);
		}

		public static PlayerTurnCommand Shoot(Vector3 aimPoint)
		{
			return new PlayerTurnCommand(PlayerTurnCommandType.Shoot, default, aimPoint);
		}
	}
}
