using System;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
using Gameplay.Navigation.Tracing;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation.Combat;
using Gameplay.Player.Runtime;
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
		[field: SerializeField, Required] public Transform Model { get; private set; }
		[SerializeField] private DiceShotDirectionView m_ShotDirectionView;
		[SerializeField] private DiceAudio m_Audio;

		private DiceState m_CurrentState;
		private GridBasis m_GridBasis;
		private int       m_ShootRange;
		private bool      m_IsInitialized;
		private bool      m_SuppressShotPreview;

		[Inject] private readonly IGameCameraController m_GameCameraController;
		[Inject] private readonly DiceShotAimService    m_ShotAimService;
		#endregion

		#region Lifecycle
		public void Initialize(int shootRange)
		{
			m_ShootRange = shootRange;
			m_Rotator.Initialize(transform, Model);
			m_IsInitialized = true;
		}

		private void Update()
		{
			if (!m_IsInitialized || m_SuppressShotPreview) {
				return;
			}

			RefreshShotDirectionPreview(true);
		}
		#endregion

		#region Positioning
		public void Snap(DiceState state, GridBasis gridBasis)
		{
			m_CurrentState = state;
			m_GridBasis    = gridBasis;

			transform.SetPositionAndRotation(GetCellPosition(state.Position, gridBasis),
			                                 gridBasis.ToWorldRotation(Quaternion.identity));
			Transform modelTransform = Model != null ? Model : transform;
			modelTransform.SetLocalPositionAndRotation(Vector3.zero, state.Orientation.GetRotation());
			RefreshShotDirectionPreview(false);
		}
		#endregion

		#region Playback
		public async UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			SetShotPreviewVisible(false);
			m_Audio?.PlayRoll();
			await m_Rotator.RollAsync(fromState, toState, direction, gridBasis);
			
			m_SuppressShotPreview = false;
			Snap(toState, gridBasis);
		}

		public async UniTask PlayShootAsync(DiceShotPresentationRequest request)
		{
			SetShotPreviewVisible(false);
			ParticleSystem[] shotVfx = ResolveShotVfx(request);
			int[]            order   = CreatePlayOrder(shotVfx.Length);
			float            elapsed = 0.0f;
			float            finish  = 0.0f;

			for (int shotIndex = 0; shotIndex < request.ShotCount; shotIndex++) {
				PlayShotFeedback(request, shotIndex);
				finish = Mathf.Max(finish, PlayShotVfx(shotVfx, order, shotIndex, elapsed));

				if (shotIndex < request.ShotCount - 1 && request.BurstDelay > 0.0f) {
					await UniTask.Delay(TimeSpan.FromSeconds(request.BurstDelay));
					elapsed += request.BurstDelay;
				}
			}

			float remainingSeconds = finish - elapsed;
			if (remainingSeconds > 0.0f) {
				await UniTask.Delay(TimeSpan.FromSeconds(remainingSeconds));
			}

			m_SuppressShotPreview = false;
			RefreshShotDirectionPreview(false);
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

		private void RefreshShotDirectionPreview(bool animateOnDirectionChange)
		{
			if (m_ShotDirectionView == null) {
				return;
			}

			if (!m_ShotAimService.TryResolvePointerShotTrace(m_CurrentState, m_ShootRange, out DiceShotDefinition shot, out GridTraceResult traceResult)) {
				m_ShotDirectionView.Hide();
				return;
			}

			int visibleDistance = GetVisibleShotPreviewDistance(traceResult);
			if (visibleDistance <= 0) {
				m_ShotDirectionView.Hide();
				return;
			}

			m_ShotDirectionView.Show(shot.Direction, visibleDistance, m_GridBasis.CellSize, animateOnDirectionChange);
		}

		private static int GetVisibleShotPreviewDistance(GridTraceResult traceResult)
		{
			int visibleDistance = traceResult.ExitedBounds ? traceResult.Distance - 1 : traceResult.Distance;
			return Mathf.Max(0, visibleDistance);
		}

		private void SetShotPreviewVisible(bool visible)
		{
			m_SuppressShotPreview = !visible;

			if (visible) {
				RefreshShotDirectionPreview(false);
				return;
			}

			m_ShotDirectionView?.Hide();
		}
		#endregion
	}
}
