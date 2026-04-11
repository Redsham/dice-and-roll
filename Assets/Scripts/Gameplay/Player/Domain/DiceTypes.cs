using System;
using UnityEngine;


namespace Gameplay.Player.Domain
{
	/// <summary>
	///     Represents the directions in which the dice can be rolled
	/// </summary>
	public enum RollDirection
	{
		North,
		East,
		South,
		West
	}

	/// <summary>
	///     Represents the values on each face of the dice
	/// </summary>
	[Serializable]
	public struct DiceOrientation : IEquatable<DiceOrientation>
	{
		public static DiceOrientation Default => new() {
			Top      = 1,
			Bottom   = 6,
			Left     = 3,
			Right    = 4,
			Forward  = 5,
			Backward = 2
		};

		public int Top;
		public int Bottom;
		public int Left;
		public int Right;
		public int Forward;
		public int Backward;


		public bool Equals(DiceOrientation other)
		{
			return Top     == other.Top     && Bottom   == other.Bottom &&
			       Left    == other.Left    && Right    == other.Right  &&
			       Forward == other.Forward && Backward == other.Backward;
		}
		public override bool Equals(object obj)
		{
			return obj is DiceOrientation other && Equals(other);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Top, Bottom, Left,
			                        Right, Forward, Backward);
		}
	}

	/// <summary>
	///     Represents the state of the dice
	/// </summary>
	[Serializable]
	public struct DiceState
	{
		public Vector2Int      Position;
		public DiceOrientation Orientation;

		public static DiceState Create(Vector2Int position)
		{
			return new() {
				Position    = position,
				Orientation = DiceOrientation.Default
			};
		}
	}
}