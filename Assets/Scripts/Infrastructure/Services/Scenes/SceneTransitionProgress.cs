namespace Infrastructure.Services.Scenes
{
	public enum SceneTransitionStage
	{
		Preparing,
		Loading,
		Activating,
		Done
	}

	public readonly struct SceneTransitionProgress
	{
		/// Name of the scene being transitioned to.
		public readonly string SceneName;
		/// Value between 0 and 1, where 0 means the transition has just started and 1 means the transition is complete.
		public readonly float Progress;
		/// The current stage of the scene transition.
		public readonly SceneTransitionStage Stage;


		public SceneTransitionProgress(string sceneName, float progress, SceneTransitionStage stage)
		{
			SceneName = sceneName;
			Progress  = progress;
			Stage     = stage;
		}
	}
}