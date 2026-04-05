using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Preferences.Ini;


namespace Preferences
{
	/// <summary>
	/// Base class for a single preferences section.
	/// </summary>
	public abstract class PreferencesCategory
	{
		// Metadata

		protected abstract string SectionName { get; }

		// Lifecycle

		/// <summary>
		/// Resets the category to its default values.
		/// </summary>
		public abstract void New();

		/// <summary>
		/// Applies the current values to Unity runtime systems.
		/// </summary>
		public abstract UniTask Apply();

		// Serialization

		protected abstract void Read(IniSectionReader  reader);
		protected abstract void Write(IniSectionWriter writer);

		/// <summary>
		/// Loads the category from its INI section.
		/// </summary>
		public virtual UniTask Load()
		{
			New();

			if (IniPreferencesStorage.TryLoadSection(SectionName, out IReadOnlyDictionary<string, string> values)) {
				Read(new IniSectionReader(values));
			}

			return UniTask.CompletedTask;
		}

		/// <summary>
		/// Saves the category into its INI section.
		/// </summary>
		public virtual UniTask Save()
		{
			IniSectionWriter writer = new();
			Write(writer);
			IniPreferencesStorage.SaveSection(SectionName, writer.Values);

			return UniTask.CompletedTask;
		}
	}
}