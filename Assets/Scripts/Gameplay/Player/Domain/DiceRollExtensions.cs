using System;
using System.Collections.Generic;
using UnityEngine;


namespace Gameplay.Player.Domain
{
	public static class DiceRollExtensions
	{
		public static DiceOrientation Roll(this DiceOrientation orientation, RollDirection direction)
		{
			return direction switch {
				RollDirection.North => new() {
					Top      = orientation.Backward,
					Bottom   = orientation.Forward,
					Left     = orientation.Left,
					Right    = orientation.Right,
					Forward  = orientation.Top,
					Backward = orientation.Bottom
				},
				RollDirection.East => new() {
					Top      = orientation.Left,
					Bottom   = orientation.Right,
					Left     = orientation.Bottom,
					Right    = orientation.Top,
					Forward  = orientation.Forward,
					Backward = orientation.Backward
				},
				RollDirection.South => new() {
					Top      = orientation.Forward,
					Bottom   = orientation.Backward,
					Left     = orientation.Left,
					Right    = orientation.Right,
					Forward  = orientation.Bottom,
					Backward = orientation.Top
				},
				RollDirection.West => new() {
					Top      = orientation.Right,
					Bottom   = orientation.Left,
					Left     = orientation.Top,
					Right    = orientation.Bottom,
					Forward  = orientation.Forward,
					Backward = orientation.Backward
				},
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}
		public static Vector2Int Move(this Vector2Int position, RollDirection direction)
		{
			return direction switch {
				RollDirection.North => position + Vector2Int.up,
				RollDirection.East  => position + Vector2Int.right,
				RollDirection.South => position + Vector2Int.down,
				RollDirection.West  => position + Vector2Int.left,
				_                   => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}

		public static Vector2Int ToVector2Int(this RollDirection direction)
		{
			return direction switch {
				RollDirection.North => Vector2Int.up,
				RollDirection.East  => Vector2Int.right,
				RollDirection.South => Vector2Int.down,
				RollDirection.West  => Vector2Int.left,
				_                   => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}

		public static Quaternion GetRotation(this DiceOrientation orientation)
		{
			Vector3 up      = GetDirectionForFace(orientation, 1);
			Vector3 right   = GetDirectionForFace(orientation, 4);
			Vector3 forward = Vector3.Cross(right, up);
			return Quaternion.LookRotation(forward, up);
		}

		private static Vector3 GetDirectionForFace(DiceOrientation orientation, int faceValue)
		{
			if (orientation.Top == faceValue) {
				return Vector3.up;
			}

			if (orientation.Bottom == faceValue) {
				return Vector3.down;
			}

			if (orientation.Left == faceValue) {
				return Vector3.left;
			}

			if (orientation.Right == faceValue) {
				return Vector3.right;
			}

			if (orientation.Forward == faceValue) {
				return Vector3.forward;
			}

			if (orientation.Backward == faceValue) {
				return Vector3.back;
			}

			throw new KeyNotFoundException($"Face '{faceValue}' was not found in the provided orientation.");
		}
	}
}
