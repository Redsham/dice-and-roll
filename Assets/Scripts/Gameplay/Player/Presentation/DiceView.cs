using System;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
using Gameplay.Camera.Runtime;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation.Combat;
using Gameplay.World.Runtime;
using TriInspector;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;


namespace Gameplay.Player.Presentation
{
	public class DiceView : MonoBehaviour, IDiceView
	{
		private const float HeightOffset = 0.5f;

		private readonly DiceRotator m_Rotator = new();

		[Title("Shoot")]
		[SerializeField] private DiceShotFaceDescriptor[] m_ShotFaces        = Array.Empty<DiceShotFaceDescriptor>();
		[SerializeField] private CameraShakeSettings      m_ShootCameraShake = CameraShakeSettings.Default;

		[Title("References")]
		[SerializeField] private DiceAudio m_Audio;

		[Inject] private readonly IGameCameraController m_GameCameraController;

		public void Initialize()
		{
			m_Rotator.Initialize(transform);
		}

		public void Snap(DiceState state, GridBasis gridBasis)
		{
			transform.SetPositionAndRotation(GetCellPosition(state.Position, gridBasis), gridBasis.ToWorldRotation(state.Orientation.GetRotation()));
		}

		public async UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			m_Audio?.PlayRoll();
			await m_Rotator.RollAsync(fromState, toState, direction, gridBasis);
			
			Snap(toState, gridBasis);
		}

		public async UniTask PlayShootAsync(DiceShotPresentationRequest request)
		{
			DiceShotFaceDescriptor descriptor        = FindDescriptor(ResolveLocalShotFace(request));
			ParticleSystem[]       shotVfx           = descriptor.ShotVfx ?? Array.Empty<ParticleSystem>();
			int[]                  playOrder         = CreatePlayOrder(shotVfx.Length);
			float                  elapsedSeconds    = 0.0f;
			float                  completionSeconds = 0.0f;

			for (int i = 0; i < request.ShotCount; i++) {
				// Audio
				m_Audio?.PlayShot((i + 1) / (float)request.ShotCount);
				
				// Camera shake
				m_GameCameraController?.Shake(m_ShootCameraShake);
				
				// Play VFX
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

		private static DiceFace ResolveLocalShotFace(DiceShotPresentationRequest request)
		{
			int shotFaceValue = request.Orientation.GetFaceValue(request.Face);

			foreach (DiceFace localFace in Enum.GetValues(typeof(DiceFace))) {
				if (DiceOrientation.Default.GetFaceValue(localFace) == shotFaceValue) {
					return localFace;
				}
			}

			return request.Face;
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

		private static Vector3 GetCellPosition(Vector2Int cell, GridBasis gridBasis)
		{
			return gridBasis.GetCellCenter(cell) + gridBasis.Up * HeightOffset;
		}
	}
}
