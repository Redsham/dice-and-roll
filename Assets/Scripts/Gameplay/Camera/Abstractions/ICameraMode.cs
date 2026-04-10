using Gameplay.Camera.Models;


namespace Gameplay.Camera.Abstractions
{
	public interface ICameraMode
	{
		void OnEnter(in CameraModeContext context);
		void OnExit();
		CameraPose Evaluate(float deltaTime, in CameraModeContext context);
	}
}
