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
			Quaternion targetRotation = to.Rotation;
			if (Quaternion.Dot(from.Rotation, targetRotation) < 0.0f) {
				targetRotation = new Quaternion(
					-targetRotation.x,
					-targetRotation.y,
					-targetRotation.z,
					-targetRotation.w
				);
			}

			return new(
			           Vector3.Lerp(from.Position, to.Position, t),
			           Quaternion.Slerp(from.Rotation, targetRotation, t)
			          );
		}
	}
}
