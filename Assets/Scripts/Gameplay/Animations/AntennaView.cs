using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Gameplay.Animations
{
	public class AntennaView : MonoBehaviour
	{
		[SerializeField] private Transform m_HorizontalJoint;
		[SerializeField] private Transform m_VerticalJoint;

		[SerializeField] private Vector2 m_HorizontalRange = new(-120f, 120f);
		[SerializeField] private Vector2 m_VerticalRange   = new(-10f, 60f);

		[SerializeField] private float m_MoveDurationMin = 4.0f;
		[SerializeField] private float m_MoveDurationMax = 7.0f;

		[SerializeField] private float m_TrackingDurationMin = 6.0f;
		[SerializeField] private float m_TrackingDurationMax = 12.0f;

		[SerializeField] private float m_IdleDelayMin = 1.0f;
		[SerializeField] private float m_IdleDelayMax = 3.0f;

		[SerializeField] private float m_MaxSpeed = 20f;
		[SerializeField] private float m_TrackingSmooth = 0.5f;

		[Header("Audio")]
		[SerializeField] private AudioSource m_AudioSource;
		[SerializeField] private float m_MaxAngularSpeed = 90f;
		[SerializeField] private float m_MinPitch = 0.8f;
		[SerializeField] private float m_MaxPitch = 1.4f;
		[SerializeField] private float m_MinVolume = 0.05f;
		[SerializeField] private float m_MaxVolume = 0.6f;
		[SerializeField] private float m_AudioSmooth = 5f;

		private float m_LastH;
		private float m_LastV;
		private float m_CurrentSpeed;
		
		private void Start()
		{
			m_LastH = GetHorizontal();
			m_LastV = GetVertical();

			if (m_AudioSource != null)
			{
				m_AudioSource.loop = true;
				if (!m_AudioSource.isPlaying)
					m_AudioSource.Play();
			}

			RunLoop(this.GetCancellationTokenOnDestroy()).Forget();
		}

		private void Update()
		{
			UpdateAudio();
		}

		private async UniTaskVoid RunLoop(CancellationToken token)
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					Vector2 target = GetRandomAngles();
					await MoveTo(target, token);

					await TrackLinear(token);

					await UniTask.Delay(
						TimeSpan.FromSeconds(UnityEngine.Random.Range(m_IdleDelayMin, m_IdleDelayMax)),
						cancellationToken: token
					);
				}
			}
			catch (OperationCanceledException) { }
		}

		private async UniTask MoveTo(Vector2 targetAngles, CancellationToken token)
		{
			float duration = UnityEngine.Random.Range(m_MoveDurationMin, m_MoveDurationMax);

			float startH = GetHorizontal();
			float startV = GetVertical();

			float targetH = NormalizeAngle(targetAngles.x);
			float targetV = NormalizeAngle(targetAngles.y);

			await UniTask.WhenAll(
				LMotion.Create(startH, targetH, duration)
					.WithEase(Ease.InOutSine)
					.Bind(SetHorizontal)
					.ToUniTask(cancellationToken: token),

				LMotion.Create(startV, targetV, duration)
					.WithEase(Ease.InOutSine)
					.Bind(SetVertical)
					.ToUniTask(cancellationToken: token)
			);
		}

		private async UniTask TrackLinear(CancellationToken token)
		{
			float duration = UnityEngine.Random.Range(m_TrackingDurationMin, m_TrackingDurationMax);
			float time = 0f;

			Vector2 start = GetRandomAngles();
			Vector2 end   = GetRandomAngles();

			while (time < duration)
			{
				token.ThrowIfCancellationRequested();

				time += Time.deltaTime;
				float t = time / duration;

				float targetH = Mathf.LerpAngle(start.x, end.x, t);
				float targetV = Mathf.LerpAngle(start.y, end.y, t);

				targetH = NormalizeAngle(targetH);
				targetV = NormalizeAngle(targetV);

				float newH = Mathf.MoveTowardsAngle(GetHorizontal(), targetH, m_MaxSpeed * Time.deltaTime);
				float newV = Mathf.MoveTowardsAngle(GetVertical(), targetV, m_MaxSpeed * Time.deltaTime);

				SetHorizontal(Mathf.LerpAngle(GetHorizontal(), newH, Time.deltaTime * m_TrackingSmooth));
				SetVertical(Mathf.LerpAngle(GetVertical(), newV, Time.deltaTime * m_TrackingSmooth));

				await UniTask.Yield(token);
			}
		}

		private void UpdateAudio()
		{
			if (m_AudioSource == null) return;

			float currentH = GetHorizontal();
			float currentV = GetVertical();

			float deltaH = Mathf.DeltaAngle(m_LastH, currentH) / Time.deltaTime;
			float deltaV = Mathf.DeltaAngle(m_LastV, currentV) / Time.deltaTime;

			float speed = (Mathf.Abs(deltaH) + Mathf.Abs(deltaV)) * 0.5f;

			m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, speed, Time.deltaTime * m_AudioSmooth);

			float normalized = Mathf.Clamp01(m_CurrentSpeed / m_MaxAngularSpeed);

			m_AudioSource.volume = Mathf.Lerp(m_MinVolume, m_MaxVolume, normalized);
			m_AudioSource.pitch  = Mathf.Lerp(m_MinPitch,  m_MaxPitch,  normalized);

			m_LastH = currentH;
			m_LastV = currentV;
		}

		private Vector2 GetRandomAngles()
		{
			return new Vector2(
				UnityEngine.Random.Range(m_HorizontalRange.x, m_HorizontalRange.y),
				UnityEngine.Random.Range(m_VerticalRange.x, m_VerticalRange.y)
			);
		}

		private float NormalizeAngle(float angle)
		{
			angle %= 360f;
			if (angle > 180f) angle -= 360f;
			return angle;
		}

		private float GetHorizontal() => NormalizeAngle(m_HorizontalJoint.localEulerAngles.y);
		private float GetVertical()   => NormalizeAngle(m_VerticalJoint.localEulerAngles.x);

		private void SetHorizontal(float value)
		{
			value = Mathf.Clamp(value, m_HorizontalRange.x, m_HorizontalRange.y);

			var rot = m_HorizontalJoint.localEulerAngles;
			rot.y = value;
			m_HorizontalJoint.localEulerAngles = rot;
		}

		private void SetVertical(float value)
		{
			value = Mathf.Clamp(value, m_VerticalRange.x, m_VerticalRange.y);

			var rot = m_VerticalJoint.localEulerAngles;
			rot.x = value;
			m_VerticalJoint.localEulerAngles = rot;
		}
	}
}