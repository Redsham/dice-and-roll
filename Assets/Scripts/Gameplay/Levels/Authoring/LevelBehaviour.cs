using Gameplay.Navigation;
using TriInspector;
using UnityEngine;


namespace Gameplay.Levels.Authoring
{
	public sealed class LevelBehaviour : MonoBehaviour
	{
		[field: SerializeField, Required] public NavGrid     NavGrid     { get; private set; } = null;
		[field: SerializeField, Required] public PlayerStart PlayerStart { get; private set; } = null;
		[field: SerializeField]           public Transform   PropsRoot   { get; private set; } = null;

		public void Initialize()
		{
			NavGrid.RebuildGrid();
		}
	}
}
