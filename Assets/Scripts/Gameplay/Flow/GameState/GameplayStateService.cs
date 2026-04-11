namespace Gameplay.Flow.GameState
{
	public sealed class GameplayStateService : IGameplayStateService
	{
		public bool              IsRunning { get; private set; }
		public bool              HasEnded  => EndReason != GameplayEndReason.None;
		public GameplayEndReason EndReason { get; private set; }

		public void Begin()
		{
			IsRunning = true;
			EndReason = GameplayEndReason.None;
		}

		public void End(GameplayEndReason reason)
		{
			if (!IsRunning || reason == GameplayEndReason.None) {
				return;
			}

			EndReason = reason;
			IsRunning = false;
		}
	}
}