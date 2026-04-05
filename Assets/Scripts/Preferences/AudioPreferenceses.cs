using Cysharp.Threading.Tasks;
using Preferences.Ini;
using UnityEngine;


namespace Preferences
{
	/// <summary>
	/// Stores audio preferences.
	/// </summary>
	public class AudioPreferenceses : PreferencesCategory
	{
		// Data

		/// <summary>
		/// Master volume multiplier.
		/// </summary>
		public float MasterVolume { get; set; }
		/// <summary>
		/// Music volume multiplier.
		/// </summary>
		public float MusicVolume { get; set; }
		/// <summary>
		/// Effects volume multiplier.
		/// </summary>
		public float EffectsVolume { get; set; }
		/// <summary>
		/// Mutes all audio when enabled.
		/// </summary>
		public bool Muted { get; set; }

		// Metadata

		protected override string SectionName => "Audio";

		// Lifecycle

		/// <inheritdoc />
		public override void New()
		{
			MasterVolume  = 1f;
			MusicVolume   = 1f;
			EffectsVolume = 1f;
			Muted         = false;
		}

		/// <inheritdoc />
		public override UniTask Apply()
		{
			AudioListener.pause  = Muted;
			AudioListener.volume = Muted ? 0f : Mathf.Clamp01(MasterVolume);

			return UniTask.CompletedTask;
		}

		// Serialization

		protected override void Read(IniSectionReader reader)
		{
			MasterVolume  = reader.GetFloat(nameof(MasterVolume),  MasterVolume);
			MusicVolume   = reader.GetFloat(nameof(MusicVolume),   MusicVolume);
			EffectsVolume = reader.GetFloat(nameof(EffectsVolume), EffectsVolume);
			Muted         = reader.GetBool(nameof(Muted), Muted);
		}

		protected override void Write(IniSectionWriter writer)
		{
			writer.Set(nameof(MasterVolume),  MasterVolume);
			writer.Set(nameof(MusicVolume),   MusicVolume);
			writer.Set(nameof(EffectsVolume), EffectsVolume);
			writer.Set(nameof(Muted),         Muted);
		}
	}
}