using System;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Player.Domain.Combat
{
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
	}
}