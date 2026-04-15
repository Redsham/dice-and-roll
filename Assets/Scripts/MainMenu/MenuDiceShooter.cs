using System;
using System.Collections;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation.Combat;
using TriInspector;
using UnityEngine;


namespace MainMenu
{
	public class MenuDiceShooter : MonoBehaviour
	{
		[Title("References")]
		[SerializeField] private MenuDiceRotator m_Rotator;
		[SerializeField] private Transform   m_ModelRoot;

		[Title("Shot")]
		[SerializeField] private DiceShotFaceDescriptor[] m_ShotFaces = Array.Empty<DiceShotFaceDescriptor>();
		[SerializeField] private float                    m_BurstDelay = 0.06f;
		[SerializeField] private float                    m_RecoilDistance = 0.08f;
		[SerializeField] private float                    m_RecoilDuration = 0.14f;

		[Title("Audio")]
		[SerializeField] private AudioSource m_ShotAudioSource;
		[SerializeField] private AudioClip   m_ShotClip;

		private Coroutine m_ShootRoutine;
		private Coroutine m_RecoilRoutine;

		private void Awake()
		{
			if (m_Rotator == null)
				m_Rotator = GetComponent<MenuDiceRotator>();

			if (m_ModelRoot == null)
				m_ModelRoot = transform;
		}

		private void OnDisable()
		{
			if (m_RecoilRoutine != null) {
				StopCoroutine(m_RecoilRoutine);
				m_RecoilRoutine = null;
			}

			m_Rotator?.SetAdditionalLocalPositionOffset(Vector3.zero);
		}

		public void Shoot(RaycastHit hit)
		{
			DiceFace face = ResolveClickedFace(hit);
			ParticleSystem[] shotVfx = FindDescriptor(face).ShotVfx ?? Array.Empty<ParticleSystem>();
			int shotCount = Mathf.Max(1, DiceOrientation.Default.GetFaceValue(face));

			if (m_ShootRoutine != null)
				StopCoroutine(m_ShootRoutine);

			m_ShootRoutine = StartCoroutine(PlayShotSequence(hit.normal, shotVfx, shotCount));
		}

		private IEnumerator PlayShotSequence(Vector3 hitNormal, ParticleSystem[] shotVfx, int shotCount)
		{
			int[] playOrder = CreatePlayOrder(shotVfx.Length);
			float elapsed = 0f;
			float finish = 0f;

			for (int shotIndex = 0; shotIndex < shotCount; shotIndex++) {
				PlayShotAudio((shotIndex + 1) / (float)shotCount);
				PlayRecoil(hitNormal);
				finish = Mathf.Max(finish, PlayShotVfx(shotVfx, playOrder, shotIndex, elapsed));

				if (shotIndex < shotCount - 1 && m_BurstDelay > 0f) {
					yield return new WaitForSecondsRealtime(m_BurstDelay);
					elapsed += m_BurstDelay;
				}
			}

			float remainingSeconds = finish - elapsed;
			if (remainingSeconds > 0f)
				yield return new WaitForSecondsRealtime(remainingSeconds);

			m_ShootRoutine = null;
		}

		private void PlayShotAudio(float t)
		{
			if (m_ShotAudioSource == null || m_ShotClip == null)
				return;

			m_ShotAudioSource.pitch = 1.0f + t * 0.2f;
			m_ShotAudioSource.PlayOneShot(m_ShotClip);
		}

		private void PlayRecoil(Vector3 hitNormal)
		{
			if (m_Rotator == null || m_RecoilDistance <= 0f || m_RecoilDuration <= 0f)
				return;

			if (m_RecoilRoutine != null)
				StopCoroutine(m_RecoilRoutine);

			Transform parentTransform = m_ModelRoot.parent;
			Vector3 localRecoilDirection = parentTransform != null
				? parentTransform.InverseTransformDirection(-hitNormal.normalized)
				: -hitNormal.normalized;
			Vector3 recoilOffset = localRecoilDirection * m_RecoilDistance;
			m_RecoilRoutine = StartCoroutine(PlayRecoilRoutine(recoilOffset));
		}

		private IEnumerator PlayRecoilRoutine(Vector3 recoilOffset)
		{
			float halfDuration = m_RecoilDuration * 0.5f;
			float elapsed = 0f;

			while (elapsed < halfDuration) {
				elapsed += Time.unscaledDeltaTime;
				float t = halfDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / halfDuration);
				m_Rotator.SetAdditionalLocalPositionOffset(Vector3.LerpUnclamped(Vector3.zero, recoilOffset, t));
				yield return null;
			}

			elapsed = 0f;
			while (elapsed < halfDuration) {
				elapsed += Time.unscaledDeltaTime;
				float t = halfDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / halfDuration);
				m_Rotator.SetAdditionalLocalPositionOffset(Vector3.LerpUnclamped(recoilOffset, Vector3.zero, t));
				yield return null;
			}

			m_Rotator.SetAdditionalLocalPositionOffset(Vector3.zero);
			m_RecoilRoutine = null;
		}

		private DiceShotFaceDescriptor FindDescriptor(DiceFace face)
		{
			for (int i = 0; i < m_ShotFaces.Length; i++) {
				if (m_ShotFaces[i].Face == face)
					return m_ShotFaces[i];
			}

			return default;
		}

		private DiceFace ResolveClickedFace(RaycastHit hit)
		{
			Vector3 localNormal = m_ModelRoot.InverseTransformDirection(hit.normal).normalized;
			DiceFace bestFace = DiceFace.Forward;
			float bestDot = float.NegativeInfinity;

			foreach (DiceFace face in Enum.GetValues(typeof(DiceFace))) {
				float dot = Vector3.Dot(GetLocalNormal(face), localNormal);
				if (dot > bestDot) {
					bestDot = dot;
					bestFace = face;
				}
			}

			return bestFace;
		}

		private static float PlayShotVfx(ParticleSystem[] shotVfx, int[] playOrder, int shotIndex, float elapsedSeconds)
		{
			if (shotVfx.Length == 0)
				return elapsedSeconds;

			ParticleSystem vfx = shotVfx[playOrder[shotIndex % playOrder.Length]];
			if (vfx == null)
				return elapsedSeconds;

			vfx.Play(true);
			return elapsedSeconds + GetDurationSeconds(vfx);
		}

		private static int[] CreatePlayOrder(int count)
		{
			if (count <= 0)
				return Array.Empty<int>();

			int[] order = new int[count];
			for (int i = 0; i < count; i++) {
				order[i] = i;
			}

			for (int i = count - 1; i > 0; i--) {
				int swapIndex = UnityEngine.Random.Range(0, i + 1);
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

		private static Vector3 GetLocalNormal(DiceFace face)
		{
			return face switch {
				DiceFace.Top      => Vector3.up,
				DiceFace.Bottom   => Vector3.down,
				DiceFace.Left     => Vector3.left,
				DiceFace.Right    => Vector3.right,
				DiceFace.Forward  => Vector3.forward,
				DiceFace.Backward => Vector3.back,
				_                 => Vector3.forward
			};
		}
	}
}
