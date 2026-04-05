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
		/// Mutes all audio when true.
		/// </summary>
		public float UIVolume { get; set; }
		/// <summary>
		/// Music volume multiplier.
		/// </summary>
		public float MusicVolume { get; set; }
		/// <summary>
		/// Effects volume multiplier.
		/// </summary>
		public float EffectsVolume { get; set; }

		// Metadata

		protected override string SectionName => "Audio";

		// Lifecycle

		/// <inheritdoc />
		public override void New()
		{
			MasterVolume  = 1f;
			MusicVolume   = 1f;
			EffectsVolume = 1f;
		}

		/// <inheritdoc />
		public override UniTask Apply() => UniTask.CompletedTask;

		// Serialization

		protected override void Read(IniSectionReader reader)
		{
			MasterVolume  = reader.GetFloat(nameof(MasterVolume),  MasterVolume);
			UIVolume      = reader.GetFloat(nameof(UIVolume),      UIVolume);
			MusicVolume   = reader.GetFloat(nameof(MusicVolume),   MusicVolume);
			EffectsVolume = reader.GetFloat(nameof(EffectsVolume), EffectsVolume);
		}

		protected override void Write(IniSectionWriter writer)
		{
			writer.Set(nameof(MasterVolume),  MasterVolume);
			writer.Set(nameof(UIVolume),      UIVolume);
			writer.Set(nameof(MusicVolume),   MusicVolume);
			writer.Set(nameof(EffectsVolume), EffectsVolume);
		}
	}
}