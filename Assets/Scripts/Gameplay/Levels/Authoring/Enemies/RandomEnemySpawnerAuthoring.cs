using System;
using Gameplay.Enemies.Authoring;
using TriInspector;
using UnityEngine;


namespace Gameplay.Levels.Authoring.Enemies
{
	public sealed class RandomEnemySpawnerAuthoring : MonoBehaviour
	{
		[Title("Spawn")]
		[field: SerializeField] public EnemyBehaviour[] EnemyPrefabs { get; private set; } = Array.Empty<EnemyBehaviour>();
		[field: SerializeField, Min(0)] public int SpawnCount { get;        private set; } = 2;

		[Title("Timing")]
		[field: SerializeField, Min(0f)] public float InitialSpawnDelay  { get; private set; } = 0.75f;
		[field: SerializeField, Min(0f)] public float DelayBetweenSpawns { get; private set; } = 0.55f;

		[Title("Spawn Effect")]
		[field: SerializeField] public GameObject SpawnEffectPrefab { get; private set; }
	}
}
