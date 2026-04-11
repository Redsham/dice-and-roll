using TriInspector;
using UnityEngine;
using UnityEngine.Localization;


namespace Gameplay.Enemies.Configs
{
	[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Gameplay/Enemies/Enemy Config")]
	public class EnemyConfig : ScriptableObject
	{
		// === Identity ===

		[Title("Identity")]
		[field: SerializeField] public string Id { get;                   private set; } = "enemy";
		[field: SerializeField] public LocalizedString DisplayName { get; private set; } = new() {
			TableReference      = "Enemies",
			TableEntryReference = "enemy_name"
		};
		[field: SerializeField] public LocalizedString Description { get; private set; } = new() {
			TableReference      = "Enemies",
			TableEntryReference = "enemy_description"
		};

		// === Stats ===

		[Title("Stats")]
		[field: SerializeField, Min(1)] public int MaxHealth { get;     private set; } = 3;
		[field: SerializeField, Min(1)] public int ContactDamage { get; private set; } = 1;

		// === Animation ===

		[Title("Animation")]
		[field: SerializeField, Min(0.01f)] public float MoveDuration { get;   private set; } = 0.2f;
		[field: SerializeField, Min(0.01f)] public float RotateDuration { get; private set; } = 0.18f;
		[field: SerializeField, Min(0.01f)] public float SpawnDuration  { get; private set; } = 0.35f;
	}
}