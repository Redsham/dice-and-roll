using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Gameplay.Levels.Data
{
	[CreateAssetMenu(fileName = "LevelAsset", menuName = "Game/Gameplay/Level Asset")]
	public sealed class LevelAsset : ScriptableObject
	{
		[field: SerializeField] public string                   Id          { get; private set; } = "level";
		[field: SerializeField] public AssetReferenceGameObject LevelPrefab { get; private set; }
	}
}