using Cysharp.Threading.Tasks;
using Gameplay.Player.Domain;
using Gameplay.World.Runtime;


namespace Gameplay.Player.Presentation
{
	public interface IDiceView
	{
		void Initialize();
		void Snap(DiceState state, GridBasis gridBasis);
		UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis);
	}
}
