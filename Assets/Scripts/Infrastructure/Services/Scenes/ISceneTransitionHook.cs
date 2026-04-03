namespace Infrastructure.Services.Scenes
{
	public interface ISceneTransitionHook
	{
		/// Called during a scene transition to report progress. The progress value is between 0 and 1, where 0 means the transition has just started and 1 means the transition is complete.
		void OnSceneTransitionProgress(SceneTransitionProgress progress);
	}
}