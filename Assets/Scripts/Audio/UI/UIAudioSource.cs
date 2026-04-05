using TriInspector;
using UnityEngine;
using UnityEngine.Audio;


namespace Audio.UI
{
	[System.Serializable]
	public class UIAudioSource
	{
		[Title("Basic")]
		public bool PlayOnAwake = false;
		public bool Loop          = false;
		public bool Mute          = false;
		public bool BypassEffects = false;

		[Title("Volume & Pitch")]
		[Range(0f,  1f)] public float Volume = 1f;
		[Range(-3f, 3f)] public float Pitch = 1f;

		[Title("Stereo")]
		[Range(-1f, 1f)] public float PanStereo = 0f;

		[Title("Spatial")]
		public bool Spatialize = false;
		public bool SpatializePostEffects = false;

		[Range(0f, 1f)] public float SpatialBlend = 0f; // 0 = 2D, 1 = 3D

		[Title("3D Settings")]
		public float DopplerLevel = 1f;
		public float Spread = 0f;

		public float MinDistance = 1f;
		public float MaxDistance = 500f;

		public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;

		[Title("Routing")]
		public AudioMixerGroup Output;

		// --- APPLY ---

		public void ApplyToSource(AudioSource source)
		{
			if (source == null)
				return;

			// Basic
			source.playOnAwake   = PlayOnAwake;
			source.loop          = Loop;
			source.mute          = Mute;
			source.bypassEffects = BypassEffects;

			// Volume / Pitch
			source.volume = Volume;
			source.pitch  = Pitch;

			// Stereo
			source.panStereo = PanStereo;

			// Spatial
			source.spatialize            = Spatialize;
			source.spatializePostEffects = SpatializePostEffects;
			source.spatialBlend          = SpatialBlend;

			// 3D
			source.dopplerLevel = DopplerLevel;
			source.spread       = Spread;

			source.minDistance = MinDistance;
			source.maxDistance = MaxDistance;
			source.rolloffMode = RolloffMode;

			// Routing
			if (Output != null)
				source.outputAudioMixerGroup = Output;
		}
	}
}