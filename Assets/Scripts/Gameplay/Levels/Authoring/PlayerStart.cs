using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	public sealed class PlayerStart : MonoBehaviour
	{
		[field: SerializeField] public Vector2Int GridPosition { get; private set; }
	}
}