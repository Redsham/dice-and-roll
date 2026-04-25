using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Models;
using LitMotion;
using LitMotion.Extensions;
using TriInspector;
using UnityEngine;
using VContainer;


namespace Gameplay.Flow.Spawning.Effects
{
	public sealed class DropPodSpawnEffect : EnemySpawnEffectBehaviour
	{
		private const float DUST_LIFETIME_PADDING = 0.25f;

		private readonly struct FlightPath
		{
			public readonly Vector3    StartPosition;
			public readonly Vector3    ThrusterPoint;
			public readonly Vector3    HoverPoint;
			public readonly Vector3    ImpactPoint;
			public readonly Vector3    TargetPosition;
			public readonly Vector3    Up;
			public readonly Quaternion Rotation;

			public FlightPath(
				Vector3    startPosition,
				Vector3    thrusterPoint,
				Vector3    hoverPoint,
				Vector3    impactPoint,
				Vector3    targetPosition,
				Vector3    up,
				Quaternion rotation
			)
			{
				StartPosition  = startPosition;
				ThrusterPoint  = thrusterPoint;
				HoverPoint     = hoverPoint;
				ImpactPoint    = impactPoint;
				TargetPosition = targetPosition;
				Up             = up;
				Rotation       = rotation;
			}
		}

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

		[Title("Camera")]
		[SerializeField, Min(1f)]    private float m_CameraStartFieldOfView = 47f;
		[SerializeField, Min(1f)]    private float m_CameraEndFieldOfView   = 52f;
		[SerializeField, Min(0.01f)] private float m_CameraFieldOfViewSmoothTime = 0.2f;
		[SerializeField]             private CameraShakeSettings m_ImpactCameraShake = new() {
			Amplitude           = 0.18f,
			Duration            = 0.22f,
			Frequency           = 24.0f,
			RotationalAmplitude = 0.8f
		};

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

		[Inject] private readonly IGameCameraController m_GameCameraController;

		[Button]
		private void AutoBind() => ResolveReferences();

		public override Transform GetCameraFocusTarget() => ResolvePodRoot();

		public override bool TryGetCameraFieldOfViewProfile(out SpawnEffectCameraFieldOfViewProfile profile)
		{
			profile = new() {
				StartFieldOfView = m_CameraStartFieldOfView,
				EndFieldOfView   = m_CameraEndFieldOfView,
				SmoothTime       = m_CameraFieldOfViewSmoothTime
			};

			return true;
		}
		public override void Prepare(in EnemySpawnEffectContext context)
		{
			Transform podRoot = ResolvePodRoot();
			if (podRoot == null) {
				throw new InvalidOperationException($"{nameof(DropPodSpawnEffect)} requires a root transform.");
			}

			FlightPath path = BuildFlightPath(context);
			ResolveReferences();
			podRoot.SetParent(context.Parent, worldPositionStays: true);
			podRoot.SetPositionAndRotation(path.StartPosition, path.Rotation);
			ResetWings();
			SetThrustersActive(false);
		}

		public override async UniTask PlayAsync(EnemySpawnEffectContext context, CancellationToken cancellationToken)
		{
			Transform podRoot = ResolvePodRoot();
			Prepare(context);
			FlightPath path   = BuildFlightPath(context);
			UniTask    wingTask = PlayWingDeployWhenReadyAsync(podRoot, path, cancellationToken);
			UniTask    fovTask  = ReportCameraFieldOfViewProgressAsync(podRoot, path, cancellationToken);

			try {
				await PlayLandingSequenceAsync(podRoot, path, cancellationToken);
				await wingTask;
				await PlayRevealSequenceAsync(path, context.Parent, cancellationToken);
				await fovTask;
			}
			finally {
				NotifyCameraFieldOfViewProgress(1.0f);
				Destroy(podRoot.gameObject);
			}
		}

		private FlightPath BuildFlightPath(in EnemySpawnEffectContext context)
		{
			Vector3 up             = context.Basis.Up;
			Vector3 targetPosition = context.TargetPosition;

			return new(
				targetPosition + up * m_InitialHeight,
				targetPosition + up * m_ThrusterStartHeight,
				targetPosition + up * m_HoverHeight,
				targetPosition - up * m_GroundSinkDepth,
				targetPosition,
				up,
				Quaternion.LookRotation(context.Basis.Forward, up)
			);
		}

		private Transform ResolvePodRoot()
		{
			if (m_PodRoot != null) {
				return m_PodRoot;
			}

			return transform.parent != null
				? transform.parent
				: transform;
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

		private async UniTask PlayWingDeployWhenReadyAsync(Transform podRoot, FlightPath path, CancellationToken cancellationToken)
		{
			if (m_Wings.Count == 0) {
				return;
			}

			float deployDistance = Vector3.Dot(path.TargetPosition + path.Up * m_WingDeployHeight, path.Up);
			while (podRoot != null && Vector3.Dot(podRoot.position, path.Up) > deployDistance) {
				cancellationToken.ThrowIfCancellationRequested();
				await UniTask.Yield(cancellationToken);
			}

			await PlayWingDeployAsync(cancellationToken);
		}

		private async UniTask PlayLandingSequenceAsync(Transform podRoot, FlightPath path, CancellationToken cancellationToken)
		{
			await MovePodAsync(podRoot, path.StartPosition, path.ThrusterPoint, m_FastDescentDuration, Ease.InQuad, cancellationToken);

			SetThrustersActive(true);
			await MovePodAsync(podRoot, podRoot.position, path.HoverPoint, m_BrakeDuration, Ease.OutCubic, cancellationToken);

			SetThrustersActive(false);
			await MovePodAsync(podRoot, podRoot.position, path.ImpactPoint, m_FinalDropDuration, Ease.InQuad, cancellationToken);
			m_GameCameraController?.Shake(m_ImpactCameraShake);
		}

		private async UniTask MovePodAsync(Transform podRoot, Vector3 from, Vector3 to, float duration, Ease ease, CancellationToken cancellationToken)
		{
			await LMotion.Create(from, to, duration)
			             .WithEase(ease)
			             .BindToPosition(podRoot)
			             .ToUniTask(cancellationToken: cancellationToken);
		}

		private async UniTask PlayRevealSequenceAsync(FlightPath path, Transform parent, CancellationToken cancellationToken)
		{
			SpawnDust(path.TargetPosition, path.Rotation, parent);
			if (m_SpawnRevealDelay <= 0f) {
				return;
			}

			await UniTask.Delay(TimeSpan.FromSeconds(m_SpawnRevealDelay), cancellationToken: cancellationToken);
		}

		private async UniTask ReportCameraFieldOfViewProgressAsync(Transform podRoot, FlightPath path, CancellationToken cancellationToken)
		{
			float totalDistance = Mathf.Max(0.001f, Vector3.Dot(path.StartPosition - path.ImpactPoint, path.Up));
			NotifyCameraFieldOfViewProgress(0.0f);

			while (podRoot != null) {
				cancellationToken.ThrowIfCancellationRequested();

				float remainingDistance = Vector3.Dot(podRoot.position - path.ImpactPoint, path.Up);
				float progress          = 1.0f - Mathf.Clamp01(remainingDistance / totalDistance);
				NotifyCameraFieldOfViewProgress(progress);

				if (progress >= 0.999f) {
					break;
				}

				await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
			}

			NotifyCameraFieldOfViewProgress(1.0f);
		}

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
			localEulerAngles.x = angle;
			wing.localEulerAngles = localEulerAngles;
		}
	}
}
