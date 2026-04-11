using System;
using Cysharp.Threading.Tasks;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation.Combat;
using Gameplay.World.Runtime;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Gameplay.Player.Presentation
{
	public class DiceView : MonoBehaviour, IDiceView
	{
		private readonly DiceRotator m_Rotator = new();

		[Title("Shoot")]
		[SerializeField] private DiceShotFaceDescriptor[] m_ShotFaces = Array.Empty<DiceShotFaceDescriptor>();

		[Title("References")]
		[SerializeField] private DiceAudio m_Audio;

		public void Initialize()
		{
			m_Rotator.Initialize(transform);
		}

		public void Snap(DiceState state, GridBasis gridBasis)
		{
			transform.SetPositionAndRotation(
			                                 gridBasis.GetCellCenter(state.Position),
			                                 gridBasis.ToWorldRotation(state.Orientation.GetRotation())
			                                );
		}

		public async UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			await m_Rotator.RollAsync(fromState, toState, direction, gridBasis);
			Snap(toState, gridBasis);
		}

		public async UniTask PlayShootAsync(DiceShotPresentationRequest request)
		{
			DiceShotFaceDescriptor descriptor        = FindDescriptor(request.Face);
			ParticleSystem[]       shotVfx           = descriptor.ShotVfx ?? Array.Empty<ParticleSystem>();
			int[]                  playOrder         = CreatePlayOrder(shotVfx.Length);
			float                  elapsedSeconds    = 0.0f;
			float                  completionSeconds = 0.0f;

			for (int i = 0; i < request.ShotCount; i++) {
				m_Audio?.PlayShot();
				if (shotVfx.Length > 0) {
					ParticleSystem vfx = shotVfx[playOrder[i % playOrder.Length]];
					if (vfx != null) {
						vfx.Play(true);
						completionSeconds = Mathf.Max(completionSeconds, elapsedSeconds + GetDurationSeconds(vfx));
					}
				}

				if (i < request.ShotCount - 1 && request.BurstDelay > 0.0f) {
					await UniTask.Delay(TimeSpan.FromSeconds(request.BurstDelay));
					elapsedSeconds += request.BurstDelay;
				}
			}

			float remainingSeconds = completionSeconds - elapsedSeconds;
			if (remainingSeconds > 0.0f) {
				await UniTask.Delay(TimeSpan.FromSeconds(remainingSeconds));
			}
		}

		private DiceShotFaceDescriptor FindDescriptor(DiceFace face)
		{
			for (int i = 0; i < m_ShotFaces.Length; i++) {
				if (m_ShotFaces[i].Face == face) {
					return m_ShotFaces[i];
				}
			}

			return default;
		}

		private static int[] CreatePlayOrder(int count)
		{
			if (count <= 0) {
				return Array.Empty<int>();
			}

			int[] order = new int[count];
			for (int i = 0; i < count; i++) {
				order[i] = i;
			}

			for (int i = count - 1; i > 0; i--) {
				int swapIndex = Random.Range(0, i + 1);
				(order[i], order[swapIndex]) = (order[swapIndex], order[i]);
			}

			return order;
		}

		private static float GetDurationSeconds(ParticleSystem particleSystem)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			float startLifetime = main.startLifetime.mode switch {
				ParticleSystemCurveMode.TwoConstants => main.startLifetime.constantMax,
				ParticleSystemCurveMode.TwoCurves    => main.startLifetime.curveMultiplier,
				_                                    => main.startLifetime.constant
			};

			return main.duration + startLifetime;
		}
	}
}