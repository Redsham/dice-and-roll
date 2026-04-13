using Gameplay.Player.Configuration;
using Gameplay.Player.Presentation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Authoring
{
	public class DiceBehaviour : MonoBehaviour
	{
		#region Inspector
		// === Grid ===
		[Title("Grid")]
		[field: SerializeField, ReadOnly] public Vector2Int GridPosition { get; set; }

		// === References ===
		[Title("References")]
		[field: SerializeField, Required] public DiceView View { get; private set; }

		// === Config ===
		[Title("Config")]
		[field: SerializeField] public DiceConfig Config { get; private set; }
		#endregion
	}
}
