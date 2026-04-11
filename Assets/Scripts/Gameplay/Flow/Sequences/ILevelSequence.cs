using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Levels.Data;


namespace Gameplay.Flow.Sequences
{
	public interface ILevelSequence
	{
		UniTask<LevelAsset> GetFirstAsync(CancellationToken cancellationToken);
		UniTask<LevelAsset> GetNextAsync(CancellationToken  cancellationToken);
	}
}