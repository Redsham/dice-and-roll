using System;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
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
		#region Constants
		private const float HEIGHT_OFFSET = 0.5f;
		#endregion

		#region Fields
		private readonly DiceRotator m_Rotator = new();

		// === Shoot ===
		[Title("Shoot")]
		[SerializeField] private DiceShotFaceDescriptor[] m_ShotFaces        = Array.Empty<DiceShotFaceDescriptor>();
		[SerializeField] private CameraShakeSettings      m_ShootCameraShake = CameraShakeSettings.Default;

		// === References ===
		[Title("References")]
		[SerializeField] private DiceAudio m_Audio;

		[Inject] private readonly IGameCameraController m_GameCameraController;
		#endregion

		#region Lifecycle
		public void Initialize()
		{
			m_Rotator.Initialize(transform);
		}
		#endregion

		#region Positioning
		public void Snap(DiceState state, GridBasis gridBasis)
		{
			transform.SetPositionAndRotation(GetCellPosition(state.Position, gridBasis), gridBasis.ToWorldRotation(state.Orientation.GetRotation()));
		}
		#endregion

		#region Playback
		public async UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			m_Audio?.PlayRoll();
			await m_Rotator.RollAsync(fromState, toState, direction, gridBasis);
			
			Snap(toState, gridBasis);
		}

		public async UniTask PlayShootAsync(DiceShotPresentationRequest request)
		{
			ParticleSystem[] shotVfx = ResolveShotVfx(request);
			int[]            order   = CreatePlayOrder(shotVfx.Length);
			float            elapsed = 0.0f;
			float            finish  = 0.0f;

			for (int shotIndex = 0; shotIndex < request.ShotCount; shotIndex++) {
				PlayShotFeedback(request, shotIndex);
				finish = Mathf.Max(finish, PlayShotVfx(shotVfx, order, shotIndex, elapsed));

				if (!ShouldWaitForNextShot(request, shotIndex)) {
					continue;
				}

				await UniTask.Delay(TimeSpan.FromSeconds(request.BurstDelay));
				elapsed += request.BurstDelay;
			}

			float remainingSeconds = finish - elapsed;
			if (remainingSeconds > 0.0f) {
				await UniTask.Delay(TimeSpan.FromSeconds(remainingSeconds));
			}
		}
		#endregion

		#region Shot Presentation
		private ParticleSystem[] ResolveShotVfx(DiceShotPresentationRequest request)
		{
			DiceFace descriptorFace = ResolveLocalShotFace(request);
			return FindDescriptor(descriptorFace).ShotVfx ?? Array.Empty<ParticleSystem>();
		}

		private void PlayShotFeedback(DiceShotPresentationRequest request, int shotIndex)
		{
			m_Audio?.PlayShot((shotIndex + 1) / (float)request.ShotCount);
			m_GameCameraController?.Shake(m_ShootCameraShake);
		}

		private static float PlayShotVfx(ParticleSystem[] shotVfx, int[] playOrder, int shotIndex, float elapsedSeconds)
		{
			if (shotVfx.Length == 0) {
				return elapsedSeconds;
			}

			ParticleSystem vfx = shotVfx[playOrder[shotIndex % playOrder.Length]];
			if (vfx == null) {
				return elapsedSeconds;
			}

			vfx.Play(true);
			return elapsedSeconds + GetDurationSeconds(vfx);
		}

		private static bool ShouldWaitForNextShot(DiceShotPresentationRequest request, int shotIndex)
		{
			return shotIndex < request.ShotCount - 1 && request.BurstDelay > 0.0f;
		}
		#endregion

		#region Shot Lookup
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
		#endregion

		#region Utilities
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
			return gridBasis.GetCellCenter(cell) + gridBasis.Up * HEIGHT_OFFSET;
		}
		#endregion
	}
}
