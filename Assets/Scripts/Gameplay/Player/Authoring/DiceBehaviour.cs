using Gameplay.Player.Configuration;
using Gameplay.Player.Presentation;
using Gameplay.Navigation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Authoring
{
	public class DiceBehaviour : MonoBehaviour, IGridPositionEntity
	{
		[Title("Grid")]
		[field: SerializeField, ReadOnly] public Vector2Int GridPosition { get; private set; }

		[Title("References")]
		[field: SerializeField, Required] public DiceView View { get; private set; }

		[Title("Config")]
		[field: SerializeField] public DiceConfig Config { get; private set; }

		public void SetGridPosition(Vector2Int gridPosition)
		{
			GridPosition = gridPosition;
		}
	}
}
