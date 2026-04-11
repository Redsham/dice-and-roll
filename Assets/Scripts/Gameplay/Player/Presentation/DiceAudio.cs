using System;
using TriInspector;
using UnityEngine;


namespace Gameplay.Player.Presentation
{
	public sealed class DiceAudio : MonoBehaviour
	{
		[Title("Shot")]
		[SerializeField] private AudioSource m_ShotAudioSource;
		[SerializeField] private AudioClip[] m_ShotClips = Array.Empty<AudioClip>();

		public void PlayShot()
		{
			if (m_ShotAudioSource == null || m_ShotClips == null || m_ShotClips.Length == 0) {
				return;
			}

			AudioClip clip = m_ShotClips[UnityEngine.Random.Range(0, m_ShotClips.Length)];
			if (clip != null) {
				m_ShotAudioSource.PlayOneShot(clip);
			}
		}
	}
}
