namespace Gameplay.Flow.GameState
{
	public interface IGameplayStateService
	{
		bool              IsRunning { get; }
		bool              HasEnded  { get; }
		GameplayEndReason EndReason { get; }
		int               TurnNumber { get; }

		void Begin();
		void AdvanceTurn();
		void End(GameplayEndReason reason);
	}
}
