using Cysharp.Threading.Tasks;
using Gameplay.Player.Domain;
using Gameplay.World.Runtime;
using LitMotion;
using UnityEngine;


namespace Gameplay.Player.Presentation
{
	public class DiceRotator
	{
		private const float RollDuration = 0.18f;

		private Transform m_Transform;

		public void Initialize(Transform targetTransform)
		{
			m_Transform = targetTransform;
		}

		public async UniTask RollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			m_Transform.SetPositionAndRotation(
			                                   gridBasis.GetCellCenter(fromState.Position),
			                                   gridBasis.ToWorldRotation(fromState.Orientation.GetRotation())
			                                  );

			Vector3 axis      = GetAxis(direction, gridBasis);
			Vector3 pivot     = GetPivot(fromState.Position, direction, gridBasis);
			float   previousT = 0.0f;

			await LMotion.Create(0.0f, 1.0f, RollDuration)
			             .WithEase(Ease.InOutQuad)
			             .Bind(t => {
				              float delta = t - previousT;
				              previousT = t;
				              m_Transform.RotateAround(pivot, axis, 90.0f * delta);
			              })
			             .ToUniTask();

			m_Transform.SetPositionAndRotation(
			                                   gridBasis.GetCellCenter(toState.Position),
			                                   gridBasis.ToWorldRotation(toState.Orientation.GetRotation())
			                                  );
		}

		private static Vector3 GetAxis(RollDirection direction, GridBasis gridBasis)
		{
			return direction switch {
				RollDirection.North => gridBasis.Right,
				RollDirection.South => -gridBasis.Right,
				RollDirection.East  => -gridBasis.Forward,
				RollDirection.West  => gridBasis.Forward,
				_                   => Vector3.zero
			};
		}

		private static Vector3 GetPivot(Vector2Int position, RollDirection direction, GridBasis gridBasis)
		{
			Vector3 center       = gridBasis.GetCellCenter(position);
			float   halfCell     = gridBasis.CellSize * 0.5f;
			Vector3 bottomOffset = -gridBasis.Up      * halfCell;

			return direction switch {
				RollDirection.North => center                + bottomOffset + gridBasis.Forward * halfCell,
				RollDirection.South => center + bottomOffset - gridBasis.Forward                * halfCell,
				RollDirection.East  => center                + bottomOffset + gridBasis.Right   * halfCell,
				RollDirection.West  => center + bottomOffset - gridBasis.Right                  * halfCell,
				_                   => center
			};
		}
	}
}