using Gameplay.Player.Configuration;
using Gameplay.Player.Presentation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Authoring
{
	public class DiceBehaviour : MonoBehaviour
	{
		[Title("Grid")]
		[field: SerializeField, ReadOnly] public Vector2Int GridPosition { get; set; }

		[Title("References")]
		[field: SerializeField, Required] public DiceView View { get; private set; }

		[Title("Config")]
		[field: SerializeField] public DiceConfig Config { get; private set; }
	}
}
