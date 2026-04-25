using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Flow.Spawning.Effects
{
	public readonly struct EnemySpawnEffectContext
	{
		public Vector2Int Cell           { get; }
		public Vector3    TargetPosition { get; }
		public GridBasis  Basis          { get; }
		public Transform  Parent         { get; }

		public EnemySpawnEffectContext(Vector2Int cell, Vector3 targetPosition, GridBasis basis, Transform parent)
		{
			Cell           = cell;
			TargetPosition = targetPosition;
			Basis          = basis;
			Parent         = parent;
		}
	}
}