using System;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Player.Domain.Combat
{
	public readonly struct DiceShotDefinition
	{
		public RollDirection Direction { get; }
		public DiceFace      Face      { get; }
		public int           ShotCount { get; }

		public DiceShotDefinition(RollDirection direction, DiceFace face, int shotCount)
		{
			Direction = direction;
			Face      = face;
			ShotCount = shotCount;
		}
	}

	public static class DiceShootExtensions
	{
		public static int GetFaceValue(this DiceOrientation orientation, DiceFace face)
		{
			return face switch {
				DiceFace.Top      => orientation.Top,
				DiceFace.Bottom   => orientation.Bottom,
				DiceFace.Left     => orientation.Left,
				DiceFace.Right    => orientation.Right,
				DiceFace.Forward  => orientation.Forward,
				DiceFace.Backward => orientation.Backward,
				_                 => throw new ArgumentOutOfRangeException(nameof(face), face, null)
			};
		}

		public static DiceFace GetFaceForDirection(this RollDirection direction)
		{
			return direction switch {
				RollDirection.North => DiceFace.Forward,
				RollDirection.East  => DiceFace.Right,
				RollDirection.South => DiceFace.Backward,
				RollDirection.West  => DiceFace.Left,
				_                   => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}

		public static bool TryGetAimDirection(this GridBasis basis, Vector3 origin, Vector3 aimPoint, out RollDirection direction)
		{
			Vector3 delta   = aimPoint - origin;
			float   right   = Vector3.Dot(delta, basis.Right);
			float   forward = Vector3.Dot(delta, basis.Forward);

			if (Mathf.Approximately(right, 0.0f) && Mathf.Approximately(forward, 0.0f)) {
				direction = default;
				return false;
			}

			if (Mathf.Abs(forward) >= Mathf.Abs(right)) {
				direction = forward >= 0.0f ? RollDirection.North : RollDirection.South;
				return true;
			}

			direction = right >= 0.0f ? RollDirection.East : RollDirection.West;
			return true;
		}

		public static bool TryResolveShot(this DiceState state, GridBasis basis, Vector3 aimPoint, out DiceShotDefinition shot)
		{
			Vector3 origin = basis.GetCellCenter(state.Position);
			if (!basis.TryGetAimDirection(origin, aimPoint, out RollDirection direction)) {
				shot = default;
				return false;
			}

			DiceFace face      = direction.GetFaceForDirection();
			int      shotCount = state.Orientation.GetFaceValue(face);
			if (shotCount <= 0) {
				shot = default;
				return false;
			}

			shot = new(direction, face, shotCount);
			return true;
		}
	}
}
