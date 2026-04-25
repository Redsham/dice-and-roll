using System;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
using Gameplay.Navigation.Tracing;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.World.Runtime;
using UnityEngine;
using VContainer;


namespace Gameplay.Player.Presentation.Combat
{
	public sealed class DiceShotBurstView : MonoBehaviour
	{
		[SerializeField] private DiceShotFaceDescriptor[] m_ShotFaces        = Array.Empty<DiceShotFaceDescriptor>();
		[SerializeField] private CameraShakeSettings      m_ShootCameraShake = CameraShakeSettings.Default;
		[SerializeField] private ParticleSystem           m_HitVfxPrefab;

		private DiceView         m_View;
		private DiceAudio        m_Audio;
		private ParticleSystem[] m_BurstShotVfx = Array.Empty<ParticleSystem>();
		private int[]            m_BurstOrder   = Array.Empty<int>();
		private int              m_BurstIndex;
		private int              m_BurstShotCount;
		private int              m_BurstShotProgress;

		[Inject] private readonly IGameCameraController m_GameCameraController;

		public void Initialize(DiceView view)
		{
			m_View  = view;
			m_Audio = view.Audio;
		}

		public void BeginBurst(DiceShotPresentationRequest request)
		{
			m_View?.HideShotPreview();
			m_BurstShotVfx      = ResolveShotVfx(request);
			m_BurstShotCount    = Mathf.Max(0, request.TotalShots);
			m_BurstShotProgress = 0;
			m_BurstIndex        = 0;
			m_BurstOrder        = CreateBurstOrder(m_BurstShotVfx.Length);

			if (m_BurstOrder.Length > 1) {
				ShuffleBurstOrder(m_BurstOrder);
			}
		}
		public void NextBurst(GridTraceResult hit, GridBasis basis)
		{
			if (m_BurstShotCount <= 0) return;

			if (m_BurstOrder.Length > 0) {
				if (m_BurstIndex >= m_BurstOrder.Length) {
					m_BurstIndex = 0;
					if (m_BurstOrder.Length > 1) {
						ShuffleBurstOrder(m_BurstOrder);
					}
				}

				ParticleSystem vfx = m_BurstShotVfx[m_BurstOrder[m_BurstIndex]];
				vfx?.Play(true);
				m_BurstIndex++;
			}

			float normalizedShotProgress = (m_BurstShotProgress + 1) / (float)m_BurstShotCount;
			m_Audio?.PlayShot(normalizedShotProgress);
			m_GameCameraController?.Shake(m_ShootCameraShake);
			m_BurstShotProgress = Mathf.Min(m_BurstShotProgress + 1, m_BurstShotCount);

			if (hit && m_HitVfxPrefab) {
				Vector3 hitPosition = basis.GetCellCenter(hit.Point) + Vector3.up * 0.5f;
				Instantiate(m_HitVfxPrefab, hitPosition, Quaternion.identity);
			}
		}
		public void EndBurst()
		{
			m_BurstShotVfx      = Array.Empty<ParticleSystem>();
			m_BurstOrder        = Array.Empty<int>();
			m_BurstIndex        = 0;
			m_BurstShotCount    = 0;
			m_BurstShotProgress = 0;
			m_View?.ShowShotPreview();
		}


		private ParticleSystem[] ResolveShotVfx(DiceShotPresentationRequest request)
		{
			DiceFace descriptorFace = ResolveLocalShotFace(request);
			return FindDescriptor(descriptorFace).ShotVfx ?? Array.Empty<ParticleSystem>();
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

		private static int[] CreateBurstOrder(int count)
		{
			if (count <= 0) {
				return Array.Empty<int>();
			}

			int[] order = new int[count];
			for (int i = 0; i < count; i++) {
				order[i] = i;
			}

			return order;
		}
		private static void ShuffleBurstOrder(int[] order)
		{
			for (int i = order.Length - 1; i > 0; i--) {
				int swapIndex = UnityEngine.Random.Range(0, i + 1);
				(order[i], order[swapIndex]) = (order[swapIndex], order[i]);
			}
		}
	}
}