using Preferences;
using UnityEngine;
using UnityEngine.Audio;


namespace Infrastructure.Services
{
	public class AudioMixerService
	{
		private const float MIN_DECIBELS = -80f;
		private const float MIN_LINEAR_VOLUME = 0.0001f;

		private const string MASTER_VOLUME_PARAM = "MasterVolume";
		private const string INTERFACE_VOLUME_PARAM = "UIVolume";
		private const string MUSIC_VOLUME_PARAM = "MusicVolume";
		private const string SFX_VOLUME_PARAM = "SFXVolume";

		private readonly AudioMixer         m_Mixer;
		private readonly PreferencesService m_Preferences;

		public AudioMixerService(AudioMixer audioMixer, PreferencesService preferences)
		{
			m_Mixer = audioMixer;
			m_Preferences = preferences;

			m_Preferences.CategoryChanged += OnCategoryChanged;
		}

		private void OnCategoryChanged(PreferencesCategory category)
		{
			if (category is not AudioPreferenceses audioPreferences) {
				return;
			}

			ApplyVolume(MASTER_VOLUME_PARAM,    audioPreferences.MasterVolume);
			ApplyVolume(INTERFACE_VOLUME_PARAM, audioPreferences.UIVolume);
			ApplyVolume(MUSIC_VOLUME_PARAM,     audioPreferences.MusicVolume);
			ApplyVolume(SFX_VOLUME_PARAM,       audioPreferences.EffectsVolume);
		}

		private void ApplyVolume(string parameterName, float volume)
		{
			float decibels = LinearToDecibels(volume);
			if (!m_Mixer.SetFloat(parameterName, decibels)) {
				Debug.LogWarning($"[{nameof(AudioMixerService)}] AudioMixer parameter '{parameterName}' was not found");
			}
		}

		private static float LinearToDecibels(float volume)
		{
			if (volume <= 0f) {
				return MIN_DECIBELS;
			}

			float clampedVolume = Mathf.Max(volume, MIN_LINEAR_VOLUME);
			return Mathf.Clamp(Mathf.Log10(clampedVolume) * 20f, MIN_DECIBELS, 0f);
		}
	}
}
