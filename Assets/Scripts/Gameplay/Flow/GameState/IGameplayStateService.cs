namespace Gameplay.Flow.GameState
{
	public interface IGameplayStateService
	{
		bool IsRunning { get; }
		bool HasEnded { get; }
		GameplayEndReason EndReason { get; }

		void Begin();
		void End(GameplayEndReason reason);
	}
}
