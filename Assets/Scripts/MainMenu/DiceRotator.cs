using TriInspector;
using UnityEngine;


namespace MainMenu
{
	public class DiceRotator : MonoBehaviour
	{
		[Title("Base Rotation")]
        [SerializeField] private Vector3 m_RotationAxis = new(0.15f, 1f, 0.08f);
        [SerializeField] private float   m_RotationSpeed = 18f;

        [Title("Idle Sway")]
        [SerializeField] private Vector3 m_SwayAngles = new(6f, 4f, 3f);
        [SerializeField] private Vector3 m_SwayFrequency = new(0.7f, 0.9f, 0.6f);

        [Title("Optional Position Float")]
        [SerializeField] private bool    m_UseFloatMotion = true;
        [SerializeField] private Vector3 m_FloatAmplitude = new(0f, 0.08f, 0f);
        [SerializeField] private float   m_FloatFrequency = 0.8f;

        [Title("Random Offset")]
        [SerializeField] private bool    m_RandomizePhase = true;

        private Quaternion _baseRotation;
        private Vector3    _basePosition;
        private Vector3    _phaseOffset;

        private void Awake()
        {
            _baseRotation = transform.localRotation;
            _basePosition = transform.localPosition;

            if (m_RandomizePhase) {
                _phaseOffset = new(
                                   Random.Range(0f, 10f),
                                   Random.Range(0f, 10f),
                                   Random.Range(0f, 10f));
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;

            Quaternion spin = Quaternion.AngleAxis(
                time * m_RotationSpeed,
                m_RotationAxis.normalized);

            Vector3 swayEuler = new(
                                    Mathf.Sin((time + _phaseOffset.x) * m_SwayFrequency.x) * m_SwayAngles.x,
                                    Mathf.Sin((time + _phaseOffset.y) * m_SwayFrequency.y) * m_SwayAngles.y,
                                    Mathf.Sin((time + _phaseOffset.z) * m_SwayFrequency.z) * m_SwayAngles.z);

            Quaternion sway = Quaternion.Euler(swayEuler);

            transform.localRotation = _baseRotation * spin * sway;

            if (m_UseFloatMotion) {
                float floatT = Mathf.Sin(time * m_FloatFrequency);
                transform.localPosition = _basePosition + m_FloatAmplitude * floatT;
            }
        }

        private void OnValidate()
        {
            if (m_RotationAxis.sqrMagnitude < 0.0001f)
                m_RotationAxis = Vector3.up;
        }
	}
}