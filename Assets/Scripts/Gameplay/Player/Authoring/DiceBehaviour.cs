using Gameplay.Player.Configuration;
using Gameplay.Player.Presentation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Authoring
{
	public class DiceBehaviour : MonoBehaviour
	{
		[Title("References")]
		[field: SerializeField, Required] public DiceView   View   { get; private set; } = null;

		[Title("Config")]
		[field: SerializeField]           public DiceConfig Config { get; private set; } = null;
	}
}
