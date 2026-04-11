using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;


namespace Audio.UI
{
	public class UISounds
	{
		// Dependencies

		[Inject] private readonly UISoundsSettings m_Settings;

		// State

		public static  bool     IsReady { get; private set; }
		private static UISounds s_Instance;

		private          AudioSource                                       m_Source;
		private readonly Dictionary<UISoundsCue, UISoundsSettings.UISound> m_SoundMap = new();

		// Constructor

		public UISounds()
		{
			if (s_Instance != null) {
				throw new($"[{nameof(UISounds)}] Multiple instances detected. There should only be one instance of {nameof(UISounds)} in the game.");
			}

			s_Instance = this;
		}

		// API

		public async UniTask Init()
		{
			if (IsReady) return;
			if (!ValidateSettings()) return;

			await LoadSounds();
			BuildMap();
			BuildSource();

			IsReady = true;

			Debug.Log($"[{nameof(UISounds)}] UISounds initialized.");
		}

		public static void Play(UISoundsCue cue)
		{
			if (!IsReady) return;
			if (!s_Instance.m_SoundMap.TryGetValue(cue, out UISoundsSettings.UISound sound)) {
				Debug.LogWarning($"[{nameof(UISounds)}] No sound found for cue: {cue}");
				return;
			}

			s_Instance.m_Source.PlayOneShot(sound.Sound.Asset as AudioClip, sound.Volume);
		}


		// Helpers

		private async UniTask LoadSounds()
		{
			UniTask<AudioClip>[] tasks = new UniTask<AudioClip>[m_Settings.Size];

			for (int i = 0; i < m_Settings.Size; i++) {
				UISoundsSettings.UISound sound = m_Settings.Sounds[i];
				tasks[i] = sound.Sound.LoadAssetAsync().ToUniTask();
			}

			await UniTask.WhenAll(tasks);
		}
		private bool ValidateSettings()
		{
			if (m_Settings == null) {
				Debug.LogError($"[{nameof(UISounds)}] UISoundsSettings is not assigned.");
				return false;
			}

			if (m_Settings.Size == 0) {
				Debug.LogWarning($"[{nameof(UISounds)}] No UI sounds defined in settings.");
			}

			if (m_Settings.Size != new HashSet<UISoundsCue>(m_Settings.Sounds.Select(s => s.Cue)).Count) {
				Debug.LogWarning($"[{nameof(UISounds)}] Duplicate UISoundsCue found in settings.");
			}

			return true;
		}
		private void BuildMap()
		{
			for (int i = 0; i < m_Settings.Size; i++) {
				UISoundsSettings.UISound sound = m_Settings.Sounds[i];
				m_SoundMap[sound.Cue] = sound;
			}
		}
		private void BuildSource()
		{
			GameObject sourceObj = new("[UIAudio]");
			Object.DontDestroyOnLoad(sourceObj);

			m_Source = sourceObj.AddComponent<AudioSource>();
			m_Settings.Source.ApplyToSource(m_Source);
		}
	}
}