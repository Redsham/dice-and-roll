using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Gameplay.Flow.Spawning.Effects
{
	[Serializable]
	public struct SpawnEffectCameraFieldOfViewProfile
	{
		[Min(1.0f)]  public float StartFieldOfView;
		[Min(1.0f)]  public float EndFieldOfView;
		[Min(0.01f)] public float SmoothTime;
	}

	public abstract class EnemySpawnEffectBehaviour : MonoBehaviour, IEnemySpawnEffect
	{
		public event Action<float> CameraFieldOfViewProgressChanged;

		public virtual void Prepare(in EnemySpawnEffectContext context)
		{
		}

		public virtual bool TryGetCameraFieldOfViewProfile(out SpawnEffectCameraFieldOfViewProfile profile)
		{
			profile = default;
			return false;
		}

		public virtual Transform GetCameraFocusTarget() => null;

		public abstract UniTask PlayAsync(EnemySpawnEffectContext context, CancellationToken cancellationToken);

		protected void NotifyCameraFieldOfViewProgress(float progress)
		{
			CameraFieldOfViewProgressChanged?.Invoke(Mathf.Clamp01(progress));
		}
	}
}
