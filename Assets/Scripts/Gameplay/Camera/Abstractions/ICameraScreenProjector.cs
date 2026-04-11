using UnityEngine;


namespace Gameplay.Camera.Abstractions
{
	public interface ICameraScreenProjector
	{
		bool TryProjectScreenPointToPlane(Vector2 screenPosition, Vector3 planeOrigin, Vector3 planeNormal, out Vector3 worldPoint);
	}
}
