using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Composition;
using Gameplay.Levels.Data;


namespace Gameplay.Flow.Sequences
{
	public sealed class SingleLevelSequence : ILevelSequence
	{
		private readonly GameplaySceneConfiguration m_Configuration;

		public SingleLevelSequence(GameplaySceneConfiguration configuration)
		{
			m_Configuration = configuration;
		}

		public UniTask<LevelAsset> GetFirstAsync(CancellationToken cancellationToken)
		{
			return UniTask.FromResult(m_Configuration.InitialLevel);
		}

		public UniTask<LevelAsset> GetNextAsync(CancellationToken cancellationToken)
		{
			return UniTask.FromResult(m_Configuration.InitialLevel);
		}
	}
}