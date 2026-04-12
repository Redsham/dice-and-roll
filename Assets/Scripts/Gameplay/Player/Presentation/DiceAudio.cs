using System;
using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Presentation
{
	public sealed class DiceAudio : MonoBehaviour
	{
		[Title("Roll")]
		[SerializeField] private AudioSource m_RollAudioSource = null;
		[SerializeField] private AudioClip[] m_RollClips = Array.Empty<AudioClip>();

		[Title("Shot")]
		[SerializeField] private AudioSource m_ShotAudioSource;
		[SerializeField] private AudioClip m_ShotClip;


		public void PlayRoll()
		{
			if (m_RollAudioSource == null || m_RollClips == null || m_RollClips.Length == 0) {
				return;
			}

			AudioClip clip = m_RollClips[UnityEngine.Random.Range(0, m_RollClips.Length)];
			if (clip != null) {
				m_RollAudioSource.PlayOneShot(clip);
			}
		}

		public void PlayShot(float t)
		{
			if (m_ShotAudioSource == null || m_ShotClip == null) {
				return;
			}

			m_ShotAudioSource.PlayOneShot(m_ShotClip);
			m_ShotAudioSource.pitch = 1.0f + t * 0.2f;
		}
	}
}