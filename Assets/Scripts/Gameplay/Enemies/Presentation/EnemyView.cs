using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Player.Domain;
using Gameplay.World.Runtime;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;


namespace Gameplay.Enemies.Presentation
{
	public class EnemyView : MonoBehaviour
	{
		// === Inspector ===

		[SerializeField] private Transform m_ModelRoot;

		// === Runtime ===

		private Transform ModelRoot => m_ModelRoot != null ? m_ModelRoot : transform;

		// === API ===

		public void Snap(Vector2Int cell, RollDirection facing, GridBasis basis)
		{
			transform.SetPositionAndRotation(
			                                 GetCellPosition(cell, basis),
			                                 Quaternion.LookRotation(ToWorldDirection(facing, basis), basis.Up)
			                                );
		}

		public async UniTask PlaySpawnAsync(Vector2Int cell, RollDirection facing, GridBasis basis, float duration, CancellationToken cancellationToken)
		{
			Snap(cell, facing, basis);

			Vector3 targetScale = ModelRoot.localScale;
			ModelRoot.localScale = Vector3.zero;

			await LMotion.Create(Vector3.zero, targetScale, duration)
			             .BindToLocalScale(ModelRoot)
			             .ToUniTask(cancellationToken: cancellationToken);
		}

		public async UniTask PlayMoveAsync(Vector2Int from, Vector2Int to, GridBasis basis, float duration, CancellationToken cancellationToken)
		{
			await LMotion.Create(GetCellPosition(from, basis), GetCellPosition(to, basis), duration)
			             .BindToPosition(transform)
			             .ToUniTask(cancellationToken: cancellationToken);
		}

		public async UniTask PlayRotateAsync(RollDirection facing, GridBasis basis, float duration, CancellationToken cancellationToken)
		{
			Quaternion targetRotation = Quaternion.LookRotation(ToWorldDirection(facing, basis), basis.Up);
			await LMotion.Create(transform.rotation, targetRotation, duration)
			             .BindToRotation(transform)
			             .ToUniTask(cancellationToken: cancellationToken);
		}

		protected static Vector3 ToWorldDirection(RollDirection direction, GridBasis basis)
		{
			return direction switch {
				RollDirection.North => basis.Forward,
				RollDirection.East  => basis.Right,
				RollDirection.South => -basis.Forward,
				RollDirection.West  => -basis.Right,
				_                   => basis.Forward
			};
		}

		private static Vector3 GetCellPosition(Vector2Int cell, GridBasis basis)
		{
			return basis.GetCellCenter(cell);
		}
	}
}
