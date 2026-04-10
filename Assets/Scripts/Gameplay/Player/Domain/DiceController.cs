namespace Gameplay.Player.Domain
{
	public sealed class DiceController
	{
		public DiceState State { get; private set; }

		public DiceController(DiceState initialState)
		{
			State = initialState;
		}

		public DiceState PreviewRoll(RollDirection direction)
		{
			return new DiceState {
				Position = State.Position.Move(direction),
				Orientation = State.Orientation.Roll(direction)
			};
		}

		public DiceState Roll(RollDirection direction)
		{
			State = PreviewRoll(direction);
			return State;
		}
	}
}
