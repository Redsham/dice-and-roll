using Cysharp.Threading.Tasks;
using Preferences.Ini;


namespace Preferences
{
	/// <summary>
	/// Placeholder for future controls preferences.
	/// </summary>
	public class ControlsPreferences : PreferenceCategory
	{
		// Metadata

		protected override string SectionName => "Controls";

		// Lifecycle

		/// <inheritdoc />
		public override void New()
		{
		}

		/// <inheritdoc />
		public override UniTask Apply()
		{
			return UniTask.CompletedTask;
		}

		// Serialization

		protected override void Read(IniSectionReader reader)
		{
		}

		protected override void Write(IniSectionWriter writer)
		{
		}
	}
}