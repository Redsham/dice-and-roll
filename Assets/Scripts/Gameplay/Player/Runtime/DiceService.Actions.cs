using Cysharp.Threading.Tasks;
using Gameplay.Navigation.Tracing;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation.Combat;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Player.Runtime
{
	public partial class DiceService
	{
		public async UniTask<bool> TryRollAsync(RollDirection direction)
		{
			if (!CanDoAction()) return false;

			DiceState nextState = m_Controller.PreviewRoll(direction);
			if (!m_NavigationService.CanOccupy(nextState.Position)) return false;

			InAction = true;

			try {
				DiceState currentState = m_Controller.State;
				DiceState appliedState = m_Controller.Roll(direction);

				await m_DiceView.PlayRollAsync(currentState, appliedState, direction, m_NavigationService.Basis);

				LeaveCell(currentState.Position);
				m_NavigationService.TryMoveEntity(m_GridActor, currentState.Position, appliedState.Position);
				EnterCell(appliedState.Position);
				return true;
			} finally {
				InAction = false;
			}
		}

		public async UniTask<bool> TryShootAsync(Vector3 aimPoint)
		{
			if (!CanDoAction()) return false;

			GridBasis basis = m_NavigationService.Basis;
			if (!m_Controller.State.TryResolveShot(basis, aimPoint, out DiceShotDefinition shot)) return false;

			InAction = true;

			try {
				DiceShotPresentationRequest request = new(m_Controller.State.Orientation, shot.Face, shot.ShotCount);
				m_DiceView.Burst.BeginBurst(request);

				for (int shotIndex = 0; shotIndex < shot.ShotCount; shotIndex++) {
					GridTraceResult traceResult = NavGridLineTrace.Trace(m_NavigationService.Grid,
					                                                     m_Controller.State.Position,
					                                                     shot.Direction.ToVector2Int(),
					                                                     m_Config.ShootRange);

					m_DiceView.Burst.NextBurst();
					traceResult.Entity?.ApplyDamage(1, m_Player.gameObject);

					if (shotIndex < shot.ShotCount - 1) {
						await UniTask.WaitForSeconds(m_Config.ShootBurstDelay);
					}
				}

				return true;
			} finally {
				m_DiceView.Burst.EndBurst();
				InAction = false;
			}
		}
	}
}