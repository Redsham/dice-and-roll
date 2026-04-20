using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Configuration
{
	[CreateAssetMenu(fileName = "DiceConfig", menuName = "Game/Gameplay/Player/Dice Config")]
	public sealed class DiceConfig : ScriptableObject
	{
		[field: Title("Stats"), SerializeField, Min(1)] public int MaxHealth { get; private set; } = 12;

		[field: Title("Shoot"), SerializeField, Min(1)]     public int   ShootRange      { get; private set; } = 2;
		[field: SerializeField, Min(                0.01f)] public float ShootBurstDelay { get; private set; } = 0.06f;


		public static DiceConfig CreateRuntimeDefault()
		{
			DiceConfig settings = CreateInstance<DiceConfig>();
			settings.hideFlags = HideFlags.DontSave;
			return settings;
		}
	}
}