using Gameplay.Player.Domain;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Player.Presentation
{
	public class DiceView : MonoBehaviour, IDiceView
	{
		private readonly DiceRotator m_Rotator = new();

		public void Initialize()
		{
			m_Rotator.Initialize(transform);
		}

		public void Snap(DiceState state, GridBasis gridBasis)
		{
			transform.SetPositionAndRotation(
				gridBasis.GetCellCenter(state.Position),
				gridBasis.ToWorldRotation(state.Orientation.GetRotation())
			);
		}

		public async Cysharp.Threading.Tasks.UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			await m_Rotator.RollAsync(fromState, toState, direction, gridBasis);
			Snap(toState, gridBasis);
		}
	}
}
