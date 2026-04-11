using UnityEngine;


namespace Gameplay.Camera.Models
{
	public readonly struct CameraModeContext
	{
		public Transform  Target      { get; }
		public CameraPose CurrentPose { get; }

		public CameraModeContext(Transform target, CameraPose currentPose)
		{
			Target      = target;
			CurrentPose = currentPose;
		}
	}
}