using Cysharp.Threading.Tasks;
using Gameplay.Player.Domain;
using Gameplay.World.Runtime;
using LitMotion;
using UnityEngine;


namespace Gameplay.Player.Presentation
{
	public class DiceRotator
	{
		private const float ROLL_DURATION = 0.3f;
		private const Ease  ROLL_EASE     = Ease.InOutCubic;

		private Transform m_RootTransform;
		private Transform m_ModelTransform;

		public void Initialize(Transform rootTransform, Transform modelTransform)
		{
			m_RootTransform  = rootTransform;
			m_ModelTransform = modelTransform != null ? modelTransform : rootTransform;
		}

		public async UniTask RollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			Quaternion baseRotation      = gridBasis.ToWorldRotation(Quaternion.identity);
			Vector3    startRootPosition = GetCellPosition(fromState.Position, gridBasis);
			Vector3    endRootPosition   = GetCellPosition(toState.Position,   gridBasis);

			m_RootTransform.SetPositionAndRotation(startRootPosition, baseRotation);
			m_ModelTransform.SetLocalPositionAndRotation(Vector3.zero, fromState.Orientation.GetRotation());

			Vector3    axis          = GetAxis(direction);
			Vector3    pivot         = GetPivot(direction, gridBasis.CellSize);
			Vector3    targetOffset  = GetTravelOffset(direction, gridBasis.CellSize);
			Vector3    modelPosition = Vector3.zero;
			Quaternion modelRotation = fromState.Orientation.GetRotation();
			float      previousT     = 0.0f;

			await LMotion.Create(0.0f, 1.0f, ROLL_DURATION)
			             .WithEase(ROLL_EASE)
			             .Bind(t => {
				              float delta = t - previousT;
				              previousT = t;

				              Quaternion deltaRotation = Quaternion.AngleAxis(90.0f * delta, axis);
				              modelPosition = pivot + deltaRotation * (modelPosition - pivot);
				              modelRotation = deltaRotation * modelRotation;

				              m_RootTransform.position = Vector3.Lerp(startRootPosition, endRootPosition, t);
				              m_ModelTransform.SetLocalPositionAndRotation(modelPosition - Vector3.Lerp(Vector3.zero, targetOffset, t), modelRotation);
			              })
			             .ToUniTask();

			m_RootTransform.SetPositionAndRotation(endRootPosition, baseRotation);
			m_ModelTransform.SetLocalPositionAndRotation(Vector3.zero, toState.Orientation.GetRotation());
		}

		private static Vector3 GetAxis(RollDirection direction)
		{
			return direction switch {
				RollDirection.North => Vector3.right,
				RollDirection.South => Vector3.left,
				RollDirection.East  => Vector3.back,
				RollDirection.West  => Vector3.forward,
				_                   => Vector3.zero
			};
		}

		private static Vector3 GetPivot(RollDirection direction, float cellSize)
		{
			float halfCell = cellSize * 0.5f;

			return direction switch {
				RollDirection.North => new(0.0f, -halfCell, halfCell),
				RollDirection.South => new(0.0f, -halfCell, -halfCell),
				RollDirection.East  => new(halfCell, -halfCell, 0.0f),
				RollDirection.West  => new(-halfCell, -halfCell, 0.0f),
				_                   => Vector3.zero
			};
		}

		private static Vector3 GetTravelOffset(RollDirection direction, float cellSize)
		{
			return direction switch {
				RollDirection.North => Vector3.forward * cellSize,
				RollDirection.South => Vector3.back    * cellSize,
				RollDirection.East  => Vector3.right   * cellSize,
				RollDirection.West  => Vector3.left    * cellSize,
				_                   => Vector3.zero
			};
		}

		private static Vector3 GetCellPosition(Vector2Int position, GridBasis gridBasis)
		{
			return gridBasis.GetCellCenter(position) + gridBasis.Up * 0.5f;
		}
	}
}