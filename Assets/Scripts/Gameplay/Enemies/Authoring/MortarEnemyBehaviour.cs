using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Presentation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Enemies.Authoring
{
	public sealed class MortarEnemyBehaviour : EnemyBehaviour
	{
		[Title("Mortar")]
		[field: SerializeField] public MortarAimMarkerView AimMarker { get; private set; } = null;

		public override Runtime.EnemyKind Kind => Runtime.EnemyKind.Mortar;
		public new MortarEnemyConfig Config => (MortarEnemyConfig)base.Config;
	}
}
