using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TriInspector;
using UnityEngine;


namespace Gameplay.Flow.Spawning.Effects
{
	public sealed class DropPodSpawnEffect : EnemySpawnEffectBehaviour
	{
		private const float DUST_LIFETIME_PADDING = 0.25f;

		[Title("References")]
		[SerializeField] private Transform m_PodRoot;
		[SerializeField] private GameObject m_DustVfxPrefab;

		[Title("Flight")]
		[SerializeField, Min(0.1f)] private float m_InitialHeight = 8f;
		[SerializeField, Min(0.1f)]  private float m_WingDeployHeight    = 2.2f;
		[SerializeField, Min(0.1f)]  private float m_ThrusterStartHeight = 1.75f;
		[SerializeField, Min(0.05f)] private float m_HoverHeight         = 0.55f;
		[SerializeField, Min(0.05f)] private float m_FastDescentDuration = 0.75f;
		[SerializeField, Min(0.05f)] private float m_BrakeDuration       = 0.35f;
		[SerializeField, Min(0.05f)] private float m_FinalDropDuration   = 0.18f;
		[SerializeField, Min(0f)]    private float m_GroundSinkDepth     = 0.08f;
		[SerializeField, Min(0f)]    private float m_SpawnRevealDelay    = 0.2f;

		[Title("Wings")]
		[SerializeField] private float m_WingClosedAngle = 0f;
		[SerializeField]             private float m_WingOpenedAngle    = 90f;
		[SerializeField, Min(0.05f)] private float m_WingDeployDuration = 0.22f;

		// === Debug ===

		[Title("Debug"), ReadOnly, ShowInInspector]
		private int m_WingCount;
		[ReadOnly, ShowInInspector]
		private int m_ThrusterCount;

		// === State ===

		private readonly List<Transform>      m_Wings          = new();
		private readonly List<Vector3>        m_WingOpenAngles = new();
		private readonly List<ParticleSystem> m_Thrusters      = new();


		[Button]
		private void AutoBind() => ResolveReferences();

		public override async UniTask PlayAsync(EnemySpawnEffectContext context, CancellationToken cancellationToken)
		{
			Transform podRoot = ResolvePodRoot();
			ResolveReferences();

			if (podRoot == null) {
				throw new InvalidOperationException($"{nameof(DropPodSpawnEffect)} requires a root transform.");
			}

			Vector3    up             = context.Basis.Up;
			Vector3    targetPosition = context.TargetPosition;
			Vector3    startPosition  = targetPosition + up * m_InitialHeight;
			Vector3    thrusterPoint  = targetPosition + up * m_ThrusterStartHeight;
			Vector3    hoverPoint     = targetPosition + up * m_HoverHeight;
			Vector3    impactPoint    = targetPosition - up * m_GroundSinkDepth;
			Quaternion upright        = Quaternion.LookRotation(context.Basis.Forward, up);

			podRoot.SetParent(context.Parent, worldPositionStays: true);
			podRoot.SetPositionAndRotation(startPosition, upright);
			ResetWings();
			SetThrustersActive(false);

			UniTask wingDeployTask = PlayWingDeployWhenReadyAsync(podRoot, targetPosition, up, m_WingDeployHeight, cancellationToken);

			try {
				await LMotion.Create(startPosition, thrusterPoint, m_FastDescentDuration)
				             .WithEase(Ease.InQuad)
				             .BindToPosition(podRoot)
				             .ToUniTask(cancellationToken: cancellationToken);

				SetThrustersActive(true);

				await LMotion.Create(podRoot.position, hoverPoint, m_BrakeDuration)
				             .WithEase(Ease.OutCubic)
				             .BindToPosition(podRoot)
				             .ToUniTask(cancellationToken: cancellationToken);

				SetThrustersActive(false);

				await LMotion.Create(podRoot.position, impactPoint, m_FinalDropDuration)
				             .WithEase(Ease.InQuad)
				             .BindToPosition(podRoot)
				             .ToUniTask(cancellationToken: cancellationToken);

				await wingDeployTask;

				SpawnDust(targetPosition, upright, context.Parent);
				if (m_SpawnRevealDelay > 0f) {
					await UniTask.Delay(TimeSpan.FromSeconds(m_SpawnRevealDelay), cancellationToken: cancellationToken);
				}
			} finally {
				Destroy(podRoot.gameObject);
			}
		}

		private Transform ResolvePodRoot()
		{
			if (m_PodRoot != null) return m_PodRoot;

			return transform.parent != null ? transform.parent : transform;
		}
		private void ResolveReferences()
		{
			Transform podRoot = ResolvePodRoot();
			m_Wings.Clear();
			m_WingOpenAngles.Clear();
			m_Thrusters.Clear();

			if (podRoot == null) {
				m_WingCount     = 0;
				m_ThrusterCount = 0;
				return;
			}

			Transform[] hierarchy = podRoot.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < hierarchy.Length; i++) {
				Transform candidate = hierarchy[i];
				if (candidate == null || candidate == transform) {
					continue;
				}

				if (candidate.name.Contains("Wing", StringComparison.OrdinalIgnoreCase)) {
					m_Wings.Add(candidate);
					Vector3 openAngles = candidate.localEulerAngles;
					openAngles.x = m_WingOpenedAngle;
					m_WingOpenAngles.Add(openAngles);
				}
			}

			ParticleSystem[] particles = podRoot.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < particles.Length; i++) {
				ParticleSystem candidate = particles[i];
				if (candidate != null && candidate.name.Contains("VFX_Thruster", StringComparison.OrdinalIgnoreCase)) {
					m_Thrusters.Add(candidate);
				}
			}

			m_WingCount     = m_Wings.Count;
			m_ThrusterCount = m_Thrusters.Count;
		}

		// === Wings ===
		
		private void ResetWings()
		{
			for (int i = 0; i < m_Wings.Count; i++) {
				Transform wing = m_Wings[i];
				if (wing == null) {
					continue;
				}

				SetWingAngle(wing, m_WingClosedAngle);
			}
		}
		private async UniTask PlayWingDeployAsync(CancellationToken cancellationToken)
		{
			if (m_Wings.Count == 0) {
				return;
			}

			UniTask[] tasks = new UniTask[m_Wings.Count];
			for (int i = 0; i < m_Wings.Count; i++) {
				Transform wing = m_Wings[i];
				if (wing == null) {
					tasks[i] = UniTask.CompletedTask;
					continue;
				}

				float targetAngle = i < m_WingOpenAngles.Count ? m_WingOpenAngles[i].x : m_WingOpenedAngle;
				tasks[i] = LMotion.Create(m_WingClosedAngle, targetAngle, m_WingDeployDuration)
				                  .WithEase(Ease.OutExpo)
				                  .Bind(value => SetWingAngle(wing, value))
				                  .ToUniTask(cancellationToken: cancellationToken);
			}

			await UniTask.WhenAll(tasks);
		}
		private async UniTask PlayWingDeployWhenReadyAsync(Transform podRoot, Vector3 targetPosition, Vector3 up, float deployHeight, CancellationToken cancellationToken)
		{
			if (m_Wings.Count == 0) return;

			float deployDistance = Vector3.Dot(targetPosition + up * deployHeight, up);
			while (podRoot != null && Vector3.Dot(podRoot.position, up) > deployDistance) {
				cancellationToken.ThrowIfCancellationRequested();
				await UniTask.Yield(cancellationToken);
			}

			await PlayWingDeployAsync(cancellationToken);
		}

		// === Thrusters ===
		
		private void SetThrustersActive(bool isActive)
		{
			for (int i = 0; i < m_Thrusters.Count; i++) {
				ParticleSystem thruster = m_Thrusters[i];
				if (thruster == null) {
					continue;
				}

				if (isActive) {
					if (!thruster.isPlaying) {
						thruster.Play(withChildren: true);
					}
				}
				else if (thruster.isPlaying) {
					thruster.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
				}
			}
		}
		
		// === VFX ===
		
		private void SpawnDust(Vector3 position, Quaternion rotation, Transform parent)
		{
			if (m_DustVfxPrefab == null) {
				return;
			}

			GameObject dustInstance = Instantiate(m_DustVfxPrefab, parent);
			dustInstance.transform.position = position;

			float lifetime = GetParticleLifetime(dustInstance) + DUST_LIFETIME_PADDING;
			Destroy(dustInstance, lifetime);
		}

		// === Utils ===
		
		private static float GetParticleLifetime(GameObject root)
		{
			ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
			float            result  = 0.5f;

			for (int i = 0; i < systems.Length; i++) {
				ParticleSystem system = systems[i];
				if (system == null) {
					continue;
				}

				ParticleSystem.MainModule main      = system.main;
				float                     candidate = main.duration + main.startLifetime.constantMax;
				result = Mathf.Max(result, candidate);
			}

			return result;
		}
		private static void SetWingAngle(Transform wing, float angle)
		{
			Vector3 localEulerAngles = wing.localEulerAngles;
			localEulerAngles.x    = angle;
			wing.localEulerAngles = localEulerAngles;
		}
	}
}