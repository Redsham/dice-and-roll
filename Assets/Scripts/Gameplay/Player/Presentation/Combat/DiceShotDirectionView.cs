using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Camera.Abstractions;
using Gameplay.Player.Domain;
using LitMotion;
using TMPro;
using TriInspector;
using UnityEngine;
using VContainer;


namespace Gameplay.Player.Presentation.Combat
{
	public sealed class DiceShotDirectionView : MonoBehaviour
	{
		private const float MIN_DISTANCE = 1.0f;
		
		[Inject] private readonly IGameCameraController m_Camera;

		[SerializeField] private GameObject m_Root;
		[SerializeField] private Transform  m_Beam;
		[SerializeField] private float      m_BeamThickness     = 0.08f;
		[SerializeField] private float      m_DirectionAnimTime = 0.18f;

		[Title("Damage")]
		[SerializeField] private TMP_Text m_DamageText;

		private RollDirection?          m_CurrentDirection;
		private int                     m_CurrentDistance;
		private float                   m_CellSize = 1.0f;
		private CancellationTokenSource m_AnimationCts;

		private GameObject Root => m_Root != null ? m_Root : gameObject;
		private Transform  Beam => m_Beam != null ? m_Beam : transform;


		private void LateUpdate()
		{
			if(m_DamageText != null) {
				m_DamageText.transform.rotation = Quaternion.LookRotation(m_DamageText.transform.position - m_Camera.Position, Vector3.up);
			}
		}

		public void Show(RollDirection direction, int distance, int damage, float cellSize, bool animate)
		{
			distance = Mathf.Max(0, distance);
			if (distance <= 0) {
				Hide();
				return;
			}

			m_CellSize = Mathf.Max(0.01f, cellSize);
			Root.SetActive(true);

			bool directionChanged = !m_CurrentDirection.HasValue || m_CurrentDirection.Value != direction;
			bool distanceChanged  = m_CurrentDistance != distance;
			m_CurrentDirection = direction;
			m_CurrentDistance  = distance;

			ApplyDirection(direction);
			ApplyDamage(damage);

			if (animate && directionChanged) {
				AnimateDistanceAsync(distance).Forget();
				return;
			}

			if (!animate || distanceChanged || m_AnimationCts == null) {
				CancelAnimation();
				ApplyDistance(distance);
			}
		}
		public void Hide()
		{
			m_CurrentDirection = null;
			m_CurrentDistance  = 0;
			CancelAnimation();
			Root.SetActive(false);
		}

		
		private async UniTaskVoid AnimateDistanceAsync(int distance)
		{
			CancelAnimation();
			m_AnimationCts = new();
			CancellationToken token = m_AnimationCts.Token;

			float startDistance = Mathf.Min(MIN_DISTANCE, distance);
			ApplyDistance(startDistance);
			
			try {
				await LMotion.Create(startDistance, distance, m_DirectionAnimTime)
				             .WithEase(Ease.OutCubic)
				             .Bind(ApplyDistance)
				             .ToUniTask(cancellationToken: token);
			} catch (OperationCanceledException) {
				return;
			}

			ApplyDistance(distance);
		}

		private void ApplyDirection(RollDirection direction)
		{
			transform.localRotation = direction switch {
				RollDirection.North => Quaternion.identity,
				RollDirection.East  => Quaternion.Euler(0.0f, 90.0f,  0.0f),
				RollDirection.South => Quaternion.Euler(0.0f, 180.0f, 0.0f),
				RollDirection.West  => Quaternion.Euler(0.0f, 270.0f, 0.0f),
				_                   => Quaternion.identity
			};
		}

		private void ApplyDistance(float distance)
		{
			float clampedDistance = Mathf.Max(MIN_DISTANCE, distance);
			float length          = clampedDistance * m_CellSize;

			if (Beam) {
				Beam.localPosition = new(0.0f, 0.0f, m_CellSize * (clampedDistance + 1.0f) * 0.5f);
				Beam.localScale    = new(m_CellSize, m_BeamThickness, length);
			}
			
			if (m_DamageText != null) {
				m_DamageText.transform.localPosition = new(0.0f, 1.0f, m_CellSize * (clampedDistance + 1.0f)  * 0.5f);
			}
		}

		private void ApplyDamage(int damage)
		{
			if(m_DamageText == null)
				return;
			
			m_DamageText.text = damage.ToString();
		}

		private void CancelAnimation()
		{
			m_AnimationCts?.Cancel();
			m_AnimationCts?.Dispose();
			m_AnimationCts = null;
		}

		private void OnDisable() => CancelAnimation();
		private void OnDestroy() => CancelAnimation();
	}
}
