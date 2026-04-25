using UnityEngine;


namespace Gameplay.Camera.Abstractions
{
	public interface ICameraScreenProjector
	{
		bool TryCreateScreenRay(Vector2 screenPosition, out Ray ray);
		bool TryProjectScreenPointToPlane(Vector2 screenPosition, Vector3 planeOrigin, Vector3 planeNormal, out Vector3 worldPoint);
		bool TryProjectWorldToScreenPoint(Vector3 worldPoint, out Vector2 screenPoint, out bool isBehindCamera);
	}
}
