using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Levels.Authoring;
using Gameplay.Levels.Data;


namespace Gameplay.Levels.Runtime
{
	public interface ILevelService
	{
		LevelAsset     CurrentAsset { get; }
		LevelBehaviour CurrentLevel { get; }
		bool           HasLevel     { get; }

		UniTask<LevelBehaviour> LoadAsync(LevelAsset    levelAsset, CancellationToken cancellationToken);
		UniTask                 ReplaceAsync(LevelAsset levelAsset, CancellationToken cancellationToken);
	}
}