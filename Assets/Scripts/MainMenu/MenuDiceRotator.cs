using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;


namespace MainMenu
{
	public class MenuDiceRotator : MonoBehaviour
	{
		[Title("Base Rotation")]
		[SerializeField] private Vector3 m_RotationAxis = new(0.15f, 1f, 0.08f);
		[SerializeField] private float m_RotationSpeed = 18f;

		[Title("Idle Sway")]
		[SerializeField] private Vector3 m_SwayAngles = new(6f, 4f, 3f);
		[SerializeField] private Vector3 m_SwayFrequency = new(0.7f, 0.9f, 0.6f);

		[Title("Optional Position Float")]
		[SerializeField] private bool m_UseFloatMotion = true;
		[SerializeField] private Vector3 m_FloatAmplitude = new(0f, 0.08f, 0f);
		[SerializeField] private float   m_FloatFrequency = 0.8f;

		[Title("Random Offset")]
		[SerializeField] private bool m_RandomizePhase = true;

		[Title("Drag")]
		[SerializeField] private float m_DragDegreesPerPixel = 0.35f;

		[Title("Idle Recovery")]
		[SerializeField] private float m_InertiaDamping = 5f;
		[SerializeField] private float m_AutoResumeAngularSpeed = 18f;
		[SerializeField] private float m_IdleResumeDelay = 0.65f;
		[SerializeField] private float m_AutoBlendDuration = 0.6f;
		[SerializeField] private float m_AutoFollowSharpness = 8f;
		[SerializeField] private float m_IdleSnapAngle = 0.1f;

		private Quaternion _baseRotation;
		private Quaternion _currentRotation;
		private Vector3    _basePosition;
		private Vector3    _additionalLocalPositionOffset;
		private Vector3    _phaseOffset;
		private Vector3    _angularVelocity;
		private Vector2    _dragStartPointerPosition;
		private Quaternion _dragStartRotation;
		private float      _idleTimer;
		private float      _autoBlend = 1f;
		private bool       _isDragging;

		private void Awake()
		{
			_baseRotation = transform.localRotation;
			_basePosition = transform.localPosition;
			_currentRotation = _baseRotation;

			if (m_RandomizePhase) {
				_phaseOffset = new(Random.Range(0f, 10f),
				                   Random.Range(0f, 10f),
				                   Random.Range(0f, 10f));
			}
		}

		private void Update()
		{
			float time = Time.unscaledTime;
			float deltaTime = Time.unscaledDeltaTime;

			Quaternion spin = Quaternion.AngleAxis(
			                                       time * m_RotationSpeed,
			                                       m_RotationAxis.normalized);

			Vector3 swayEuler = new(
			                        Mathf.Sin((time + _phaseOffset.x) * m_SwayFrequency.x) * m_SwayAngles.x,
			                        Mathf.Sin((time + _phaseOffset.y) * m_SwayFrequency.y) * m_SwayAngles.y,
			                        Mathf.Sin((time + _phaseOffset.z) * m_SwayFrequency.z) * m_SwayAngles.z);

			Quaternion sway = Quaternion.Euler(swayEuler);
			Quaternion autoRotation = _baseRotation * spin * sway;

			UpdateRotationState(autoRotation, deltaTime);

			transform.localRotation = _currentRotation;

			if (m_UseFloatMotion) {
				float floatT = Mathf.Sin(time * m_FloatFrequency);
				transform.localPosition = _basePosition + m_FloatAmplitude * floatT + _additionalLocalPositionOffset;
			}
			else {
				transform.localPosition = _basePosition + _additionalLocalPositionOffset;
			}
		}

		private void OnValidate()
		{
			if (m_RotationAxis.sqrMagnitude < 0.0001f)
				m_RotationAxis = Vector3.up;

			m_DragDegreesPerPixel = Mathf.Max(0.01f, m_DragDegreesPerPixel);
			m_InertiaDamping = Mathf.Max(0f, m_InertiaDamping);
			m_AutoResumeAngularSpeed = Mathf.Max(0f, m_AutoResumeAngularSpeed);
			m_IdleResumeDelay = Mathf.Max(0f, m_IdleResumeDelay);
			m_AutoBlendDuration = Mathf.Max(0.01f, m_AutoBlendDuration);
			m_AutoFollowSharpness = Mathf.Max(0.01f, m_AutoFollowSharpness);
			m_IdleSnapAngle = Mathf.Max(0.001f, m_IdleSnapAngle);
		}

		private void OnDisable()
		{
			_isDragging = false;
			_angularVelocity = Vector3.zero;
			_idleTimer = 0f;
			_autoBlend = 1f;
			_additionalLocalPositionOffset = Vector3.zero;
			transform.localPosition = _basePosition;
		}

		public void SetAdditionalLocalPositionOffset(Vector3 offset)
		{
			_additionalLocalPositionOffset = offset;
		}

		public void BeginDrag(Vector2 pointerPosition)
		{
			_isDragging = true;
			_dragStartPointerPosition = pointerPosition;
			_dragStartRotation = _currentRotation;
			_angularVelocity = Vector3.zero;
			_idleTimer = 0f;
			_autoBlend = 0f;
		}

		public void DragTo(Vector2 pointerPosition, Camera inputCamera, float deltaTime)
		{
			if (!_isDragging || inputCamera == null)
				return;

			Quaternion targetRotation = GetDragRotation(_dragStartRotation, _dragStartPointerPosition, pointerPosition, inputCamera);
			Quaternion deltaRotation = targetRotation * Quaternion.Inverse(_currentRotation);
			_currentRotation = targetRotation;
			_angularVelocity = Vector3.Lerp(_angularVelocity,
			                                GetAngularVelocity(deltaRotation, deltaTime),
			                                1f - Mathf.Exp(-18f * deltaTime));
		}

		public void EndDrag()
		{
			_isDragging = false;
		}

		private void UpdateRotationState(Quaternion autoRotation, float deltaTime)
		{
			if (_isDragging)
				return;

			if (_angularVelocity.sqrMagnitude > 0.0001f) {
				float angularSpeed = _angularVelocity.magnitude;
				Vector3 axis = _angularVelocity / angularSpeed;
				_currentRotation = Quaternion.AngleAxis(angularSpeed * deltaTime, axis) * _currentRotation;
				_angularVelocity = Vector3.Lerp(_angularVelocity,
				                                Vector3.zero,
				                                1f - Mathf.Exp(-m_InertiaDamping * deltaTime));
			}
			else {
				_angularVelocity = Vector3.zero;
			}

			if (_angularVelocity.magnitude <= m_AutoResumeAngularSpeed)
				_idleTimer += deltaTime;
			else
				_idleTimer = 0f;

			float targetBlend = _idleTimer >= m_IdleResumeDelay ? 1f : 0f;
			_autoBlend = Mathf.MoveTowards(_autoBlend, targetBlend, deltaTime / m_AutoBlendDuration);

			if (_autoBlend <= 0f)
				return;

			float followFactor = 1f - Mathf.Exp(-m_AutoFollowSharpness * _autoBlend * deltaTime);
			_currentRotation = Quaternion.Slerp(_currentRotation, autoRotation, followFactor);

			if (_autoBlend >= 0.999f
			    && _angularVelocity.sqrMagnitude <= 0.0001f
			    && Quaternion.Angle(_currentRotation, autoRotation) <= m_IdleSnapAngle)
				_currentRotation = autoRotation;
		}

		private Quaternion GetDragRotation(Quaternion startRotation, Vector2 dragStartPointer, Vector2 currentPointer, Camera inputCamera)
		{
			Vector2 pointerDelta = currentPointer - dragStartPointer;
			if (pointerDelta.sqrMagnitude <= Mathf.Epsilon)
				return startRotation;

			float yawAngle = -pointerDelta.x * m_DragDegreesPerPixel;
			float pitchAngle = pointerDelta.y * m_DragDegreesPerPixel;

			Transform parentTransform = transform.parent;
			Vector3 localUpAxis = GetAxisInLocalSpace(inputCamera.transform.up, parentTransform);
			Vector3 localRightAxis = GetAxisInLocalSpace(inputCamera.transform.right, parentTransform);

			Quaternion yawRotation = Quaternion.AngleAxis(yawAngle, localUpAxis);
			Quaternion pitchRotation = Quaternion.AngleAxis(pitchAngle, localRightAxis);

			return yawRotation * pitchRotation * startRotation;
		}

		private Vector3 GetAngularVelocity(Quaternion deltaRotation, float deltaTime)
		{
			if (deltaTime <= Mathf.Epsilon)
				return Vector3.zero;

			deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

			if (float.IsNaN(axis.x) || axis.sqrMagnitude < 0.0001f)
				return Vector3.zero;

			if (angle > 180f)
				angle -= 360f;

			return axis.normalized * (angle / deltaTime);
		}

		private static Vector3 GetAxisInLocalSpace(Vector3 worldAxis, Transform parentTransform)
		{
			if (parentTransform == null)
				return worldAxis.normalized;

			return parentTransform.InverseTransformDirection(worldAxis).normalized;
		}
	}
}
