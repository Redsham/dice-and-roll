using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Presentation;
using Gameplay.Enemies.Runtime;
using TriInspector;
using UnityEngine;


namespace Gameplay.Enemies.Authoring
{
	public sealed class MortarEnemyBehaviour : EnemyBehaviour
	{
		[Title("Mortar")]
		[field: SerializeField] public MortarAimMarkerView AimMarker { get; private set; }

		public override EnemyKind         Kind   => EnemyKind.Mortar;
		public new      MortarEnemyConfig Config => (MortarEnemyConfig)base.Config;
	}
}