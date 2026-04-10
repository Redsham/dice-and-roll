using UnityEngine;


namespace Gameplay.Camera.Models
{
	public readonly struct CameraPose
	{
		public Vector3    Position { get; }
		public Quaternion Rotation { get; }

		public CameraPose(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}

		public static CameraPose Lerp(in CameraPose from, in CameraPose to, float t)
		{
			return new CameraPose(
				Vector3.Lerp(from.Position, to.Position, t),
				Quaternion.Slerp(from.Rotation, to.Rotation, t)
			);
		}
	}
}
