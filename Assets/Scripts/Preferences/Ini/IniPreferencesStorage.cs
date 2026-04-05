using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Preferences.Ini
{
	/// <summary>
	/// File-based storage for preferences INI sections.
	/// </summary>
	public static class IniPreferencesStorage
	{
		private const string FILE_NAME = "Preferences.ini";

		/// <summary>
		/// Absolute path to the preferences INI file.
		/// </summary>
		public static string FilePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

		/// <summary>
		/// Returns <c>true</c> when the preferences file already exists.
		/// </summary>
		public static bool Exists() => File.Exists(FilePath);

		/// <summary>
		/// Tries to load a section from the preferences file.
		/// </summary>
		public static bool TryLoadSection(string section, out IReadOnlyDictionary<string, string> values)
		{
			IniDocument document = LoadDocument();
			return document.TryGetSection(section, out values);
		}

		/// <summary>
		/// Saves a single section into the preferences file.
		/// </summary>
		public static void SaveSection(string section, IReadOnlyDictionary<string, string> values)
		{
			IniDocument document = LoadDocument();
			document.SetSection(section, values);

			string directory = Path.GetDirectoryName(FilePath);
			if (!string.IsNullOrEmpty(directory)) {
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(FilePath, document.Serialize());
		}

		// Helpers

		private static IniDocument LoadDocument()
		{
			if (!Exists()) {
				return new();
			}

			return IniDocument.Parse(File.ReadAllText(FilePath));
		}
	}
}